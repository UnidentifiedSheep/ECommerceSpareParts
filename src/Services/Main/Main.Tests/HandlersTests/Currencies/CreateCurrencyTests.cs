using Main.Abstractions.Constants;
using Main.Application.Handlers.Currencies.CreateCurrency;
using Microsoft.EntityFrameworkCore;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders;
using ValidationException = FluentValidation.ValidationException;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.Currencies;

[Collection("Combined collection")]
public class CreateCurrencyTests(CombinedContainerFixture fixture) : Test(fixture)
{
    private string GetValidName()
    {
        return Global.Faker.Lorem.Letter(10);
    }

    private string GetValidCurrencyCode()
    {
        return Global.Faker.Lorem.Letter(2);
    }

    private string GetValidCurrencySign()
    {
        return Global.Faker.Lorem.Letter();
    }

    private string GetValidShortName()
    {
        return Global.Faker.Lorem.Letter(3);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("d")]
    [InlineData("jjjjjjjjjjjjjjjjjjjjjjjjjjjjjj")]
    public async Task CreateCurrency_WithInvalidShortName_ThrowsValidationException(string shortName)
    {
        var command =
            new CreateCurrencyCommand(shortName, GetValidName(), GetValidCurrencySign(), GetValidCurrencyCode());
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("d")]
    [InlineData("tooBig")]
    public async Task CreateCurrency_WithInvalidName_ThrowsValidationException(string name)
    {
        if (name == "tooBig") name = Faker.Lorem.Letter(200);
        var command = new CreateCurrencyCommand(
            GetValidShortName(), 
            name, 
            GetValidCurrencySign(), 
            GetValidCurrencyCode());
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("sdfsddddd")]
    public async Task CreateCurrency_WithInvalidSign_ThrowsValidationException(string sign)
    {
        var command = new CreateCurrencyCommand(GetValidShortName(), GetValidName(), sign, GetValidCurrencyCode());
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("d")]
    [InlineData("sdfsddiimimimimiimimiiimmmmmimimiimmmiiimim")]
    public async Task CreateCurrency_WithInvalidCode_ThrowsValidationException(string code)
    {
        var command = new CreateCurrencyCommand(GetValidShortName(), GetValidName(), GetValidCurrencySign(), code);
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task CreateCurrency_WithDuplicateData_ThrowsErrors()
    {
        var created = await new CurrencyBuilder(Faker)
            .BuildAndAddToDb(Context);
        
        var exception = await Assert.ThrowsAsync<DbValidationException>(async () =>
            await Mediator.Send(new CreateCurrencyCommand(GetValidShortName(), GetValidName(), GetValidCurrencySign(),
                created.Code)));
        
        Assert.Equal(ApplicationErrors.CurrencyCodeAlreadyTaken, exception.Failures[0].ErrorName);

        exception = await Assert.ThrowsAsync<DbValidationException>(async () =>
            await Mediator.Send(new CreateCurrencyCommand(GetValidShortName(), created.Name, GetValidCurrencySign(),
                GetValidCurrencyCode())));
        
        Assert.Equal(ApplicationErrors.CurrencyNameAlreadyTaken, exception.Failures[0].ErrorName);

        exception = await Assert.ThrowsAsync<DbValidationException>(async () =>
            await Mediator.Send(new CreateCurrencyCommand(GetValidShortName(), GetValidName(), created.CurrencySign,
                GetValidCurrencyCode())));
        
        Assert.Equal(ApplicationErrors.CurrencySignAlreadyTaken, exception.Failures[0].ErrorName);

        exception = await Assert.ThrowsAsync<DbValidationException>(async () =>
            await Mediator.Send(new CreateCurrencyCommand(created.ShortName, GetValidName(), GetValidCurrencySign(),
                GetValidCurrencyCode())));
        
        Assert.Equal(ApplicationErrors.CurrencyShortNameAlreadyTaken, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task CreateCurrency_WithValidData_Succeeds()
    {
        var shortName = GetValidShortName();
        var name = GetValidName();
        var code = GetValidCurrencyCode();
        var sign = GetValidCurrencySign();

        await Mediator.Send(new CreateCurrencyCommand(shortName, name, sign, code));

        var currencyInDb = await Context.Currencies.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(currencyInDb);
        Assert.Equal(shortName.Trim(), currencyInDb.ShortName);
        Assert.Equal(name.Trim(), currencyInDb.Name);
        Assert.Equal(code.Trim(), currencyInDb.Code);
        Assert.Equal(sign.Trim(), currencyInDb.CurrencySign);
    }
}