using FluentAssertions;
using Main.Entities.Currency;
using Main.Modules.Currency.Application.Commands.Currencies.CreateCurrency;
using Main.Modules.Currency.Application.Dtos;
using Main.Modules.Currency.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Currency;

namespace Tests.CacheRepositoriesTests;

public class CurrencyCacheRepositoryTests : IntegrationTest
{
	public CurrencyCacheRepositoryTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<CurrencyTestContext>();

	private CurrencyTestContext TestContext => GetContext<CurrencyTestContext>();

	[Fact]
	public async Task GetCurrency_WhenCurrencyExists_ReturnsCurrency()
	{
		var currency = TestContext.Currencies[0];
		var repository = GetRepository();
		await repository.InvalidateAllCurrencies(CancellationToken);

		var result = await repository.GetCurrency(currency.Id, CancellationToken);

		result.Should().NotBeNull();
		AssertCurrency(result!, currency);
	}

	[Fact]
	public async Task GetCurrency_WhenCurrencyDoesNotExist_ReturnsNull()
	{
		var repository = GetRepository();
		await repository.InvalidateAllCurrencies(CancellationToken);

		var result = await repository.GetCurrency(int.MaxValue, CancellationToken);

		result.Should().BeNull();
	}

	[Fact]
	public async Task GetAllCurrencies_ReturnsEveryCurrency()
	{
		var currencies = TestContext.Currencies;
		var repository = GetRepository();
		await repository.InvalidateAllCurrencies(CancellationToken);

		var result = await repository.GetAllCurrencies(CancellationToken);

		result.Select(x => x.Id).Should().BeEquivalentTo(currencies.Select(x => x.Id));
		foreach (var currency in currencies)
			AssertCurrency(result.Single(x => x.Id == currency.Id), currency);
	}

	[Fact]
	public async Task SecondRead_IsServedFromCache()
	{
		var currency = TestContext.Currencies[0];
		var repository = GetRepository();
		await repository.InvalidateAllCurrencies(CancellationToken);

		var cached = await repository.GetCurrency(currency.Id, CancellationToken);
		await UpdateCurrencyName(currency.Id, "Updated currency name");

		var single = await repository.GetCurrency(currency.Id, CancellationToken);
		var all = await repository.GetAllCurrencies(CancellationToken);

		single.Should().BeEquivalentTo(cached);
		all.Single(x => x.Id == currency.Id).Name.Should().Be(cached!.Name);
	}

	[Fact]
	public async Task InvalidateAllCurrencies_ReloadsCurrenciesFromDb()
	{
		var currency = TestContext.Currencies[0];
		var repository = GetRepository();
		await repository.InvalidateAllCurrencies(CancellationToken);
		await repository.GetAllCurrencies(CancellationToken);

		await UpdateCurrencyName(currency.Id, "Invalidated currency");
		await repository.InvalidateAllCurrencies(CancellationToken);

		var result = await repository.GetCurrency(currency.Id, CancellationToken);

		result!.Name.Should().Be("Invalidated currency");
	}

	[Fact]
	public async Task CreatedCurrency_IsVisibleThroughCache()
	{
		var repository = GetRepository();
		await repository.InvalidateAllCurrencies(CancellationToken);
		await repository.GetAllCurrencies(CancellationToken);

		var created = await Mediator.Send(
			new CreateCurrencyCommand("CCH", "Cache currency", "C", "CACHE"),
			CancellationToken);

		var result = await repository.GetCurrency(created.Id, CancellationToken);

		result.Should().NotBeNull();
		result!.Code.Should().Be("CACHE");
	}

	private ICurrencyCacheRepository GetRepository()
		=> Scope.ServiceProvider.GetRequiredService<ICurrencyCacheRepository>();

	private async Task UpdateCurrencyName(int currencyId, string name)
	{
		await Context.Database.ExecuteSqlInterpolatedAsync(
			$"""UPDATE currency SET name = {name} WHERE id = {currencyId}""");
		Context.ChangeTracker.Clear();
	}

	private static void AssertCurrency(CurrencyDto dto, Currency currency)
	{
		dto.Id.Should().Be(currency.Id);
		dto.Name.Should().Be(currency.Name);
		dto.ShortName.Should().Be(currency.ShortName);
		dto.CurrencySign.Should().Be(currency.CurrencySign);
		dto.Code.Should().Be(currency.Code);
	}
}
