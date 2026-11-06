using Main.Entities;
using Tests.Abstractions.Test;
using FluentValidation.TestHelper;
using Main.Entities.Exceptions;
using Main.Application.Handlers.Currencies.CreateCurrency;
using Main.Application.Static;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders.Currency;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Currencies;

public class CreateCurrencyTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	private string GetValidName() => Faker.Lorem.Letter(10);

	private string GetValidCurrencyCode() => Faker.Lorem.Letter(2);

	private string GetValidCurrencySign() => Faker.Lorem.Letter();

	private string GetValidShortName() => Faker.Lorem.Letter(3);

	[Fact]
	public async Task CreateCurrency_WithDuplicateData_ThrowsErrors()
	{
		var created = await new CurrencyBuilder(Faker).BuildAndAddToDb(Context);

		var exception = await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(
			new CreateCurrencyCommand(
				GetValidShortName(),
				GetValidName(),
				GetValidCurrencySign(),
				created.Code),
			CancellationToken));

		Assert.IsType<CurrencyCodeAlreadyTakenException>(exception.Failures[0].Error);

		exception = await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(
			new CreateCurrencyCommand(
				GetValidShortName(),
				created.Name,
				GetValidCurrencySign(),
				GetValidCurrencyCode()),
			CancellationToken));

		Assert.IsType<CurrencyNameAlreadyTakenException>(exception.Failures[0].Error);

		exception = await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(
			new CreateCurrencyCommand(
				GetValidShortName(),
				GetValidName(),
				created.CurrencySign,
				GetValidCurrencyCode()),
			CancellationToken));

		Assert.IsType<CurrencySignAlreadyTakenException>(exception.Failures[0].Error);

		exception = await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(
			new CreateCurrencyCommand(
				created.ShortName,
				GetValidName(),
				GetValidCurrencySign(),
				GetValidCurrencyCode()),
			CancellationToken));

		Assert.IsType<CurrencyShortNameAlreadyTakenException>(exception.Failures[0].Error);
	}

	[Theory]
	[InlineData("code")]
	[InlineData("name")]
	[InlineData("shortName")]
	[InlineData("sign")]
	public async Task DuplicateTrimmedField_ReturnsItsLocalizedError(string field)
	{
		var currency = await new CurrencyBuilder(Faker).WithShortName("TT").WithSign("!").BuildAndAddToDb(Context);
		var command = new CreateCurrencyCommand(
			field == "shortName" ? $" {currency.ShortName} " : "QQ",
			field == "name" ? $" {currency.Name} " : "Another currency",
			field == "sign" ? $" {currency.CurrencySign} " : "?",
			field == "code" ? $" {currency.Code} " : "ZZZ");

		var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command, CancellationToken));

		var expected = field switch
		{
			"code" => typeof(CurrencyCodeAlreadyTakenException),
			"name" => typeof(CurrencyNameAlreadyTakenException),
			"shortName" => typeof(CurrencyShortNameAlreadyTakenException),
			_ => typeof(CurrencySignAlreadyTakenException)
		};
		Assert.Contains(exception.Failures, failure => failure.Error?.GetType() == expected);
	}

	[Fact]
	public async Task CreateCurrency_WithValidData_Succeeds()
	{
		var shortName = GetValidShortName();
		var name = GetValidName();
		var code = GetValidCurrencyCode();
		var sign = GetValidCurrencySign();

		await Mediator.Send(
			new CreateCurrencyCommand(
				shortName,
				name,
				sign,
				code),
			CancellationToken);

		var currencyInDb = await Context.Currencies.AsNoTracking().FirstOrDefaultAsync(CancellationToken);
		Assert.NotNull(currencyInDb);
		Assert.Equal(shortName.Trim(), currencyInDb.ShortName);
		Assert.Equal(name.Trim(), currencyInDb.Name);
		Assert.Equal(code.Trim(), currencyInDb.Code);
		Assert.Equal(sign.Trim(), currencyInDb.CurrencySign);
	}
}

public class CreateCurrencyValidationTests : ValidationTest<CreateCurrencyValidation, CreateCurrencyCommand>
{
	[Theory]
	[InlineData("")]
	[InlineData("  ")]
	[InlineData("d")]
	[InlineData("jjjjjjjjjjjjjjjjjjjjjjjjjjjjjj")]
	public async Task CreateCurrency_WithInvalidShortName_FailsValidation(string shortName)
	{
		var result = await ValidateAsync(new CreateCurrencyCommand(shortName, "US dollar", "$", "USD"));
		result.ShouldHaveValidationErrorFor(x => x.ShortName).Only();
	}

	[Theory]
	[InlineData("")]
	[InlineData("  ")]
	[InlineData("d")]
	[InlineData("tooBig")]
	public async Task CreateCurrency_WithInvalidName_FailsValidation(string name)
	{
		var result = await ValidateAsync(new CreateCurrencyCommand("USD", name == "tooBig" ? new string('x', 200) : name, "$", "USD"));
		result.ShouldHaveValidationErrorFor(x => x.Name).Only();
	}

	[Theory]
	[InlineData("")]
	[InlineData("  ")]
	[InlineData("sdfsddddd")]
	public async Task CreateCurrency_WithInvalidSign_FailsValidation(string sign)
	{
		var result = await ValidateAsync(new CreateCurrencyCommand("USD", "US dollar", sign, "USD"));
		result.ShouldHaveValidationErrorFor(x => x.CurrencySign).Only();
	}

	[Theory]
	[InlineData("")]
	[InlineData("  ")]
	[InlineData("d")]
	[InlineData("sdfsddiimimimimiimimiiimmmmmimimiimmmiiimim")]
	public async Task CreateCurrency_WithInvalidCode_FailsValidation(string code)
	{
		var result = await ValidateAsync(new CreateCurrencyCommand("USD", "US dollar", "$", code));
		result.ShouldHaveValidationErrorFor(x => x.Code).Only();
	}
}
