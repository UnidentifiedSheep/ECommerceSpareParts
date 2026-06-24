using Cache;
using FluentAssertions;
using Main.Application.Dtos.Currencies;
using Main.Application.Interfaces.Cache;
using Main.Application.Static;
using Main.Entities.Currency;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts.Currency;

namespace Tests.CacheRepositoriesTests;

public class CurrencyCacheRepositoryTests : IntegrationTest
{
    public CurrencyCacheRepositoryTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<CurrencyTestContext>();
    }

    private CurrencyTestContext TestContext => GetContext<CurrencyTestContext>();

    [Fact]
    public async Task GetCurrency_WhenCurrencyExists_ReturnsCurrencyFromDbAndCaches()
    {
        var currency = TestContext.Currencies[0];
        var repository = GetRepository();

        await RemoveCachedCurrency(currency.Id);

        var result = await repository.GetCurrency(currency.Id);

        result.Should().NotBeNull();
        AssertCurrency(result!, currency);

        var cached = await GetCache().GetAsync<CurrencyDto>(CacheKeys.CurrencyCache.Currency(currency.Id));
        cached.Should().BeEquivalentTo(result);
    }

    [Fact]
    public async Task GetCurrency_WhenCurrencyAlreadyCached_ReturnsCachedCurrency()
    {
        var currency = TestContext.Currencies[0];
        var repository = GetRepository();

        await RemoveCachedCurrency(currency.Id);

        var cached = await repository.GetCurrency(currency.Id);

        await UpdateCurrencyName(currency.Id, "Updated currency name");

        var result = await repository.GetCurrency(currency.Id);

        result.Should().BeEquivalentTo(cached);
        result!.Name.Should().NotBe("Updated currency name");
    }

    [Fact]
    public async Task GetCurrency_WhenCurrencyDoesNotExist_ReturnsNull()
    {
        var repository = GetRepository();

        var result = await repository.GetCurrency(int.MaxValue);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllCurrencies_WhenCurrenciesSetNotCached_ReturnsCurrenciesFromDbAndCachesSet()
    {
        var currencies = TestContext.Currencies;
        var repository = GetRepository();

        await RemoveAllCachedCurrencies(currencies.Select(x => x.Id));

        var result = await repository.GetAllCurrencies();

        result.Should().HaveCount(currencies.Count);
        foreach (var currency in currencies)
            AssertCurrency(result.Single(x => x.Id == currency.Id), currency);

        var cachedIds = await GetCache().GetFromSetAsync(CacheKeys.CurrencyCache.AllCurrencies());
        cachedIds.Select(int.Parse).Should().BeEquivalentTo(currencies.Select(x => x.Id));
    }

    [Fact]
    public async Task GetAllCurrencies_WhenCurrencySetCached_ReturnsCachedAndMissingCurrencies()
    {
        var cachedCurrency = TestContext.Currencies[0];
        var dbCurrency = TestContext.Currencies[1];
        var repository = GetRepository();

        await RemoveAllCachedCurrencies(TestContext.Currencies.Select(x => x.Id));

        await repository.GetAllCurrencies();
        var cachedBeforeUpdate = await repository.GetCurrency(cachedCurrency.Id);

        await UpdateCurrencyName(cachedCurrency.Id, "Cached updated name");
        await UpdateCurrencyName(dbCurrency.Id, "Db updated name");

        var result = await repository.GetAllCurrencies();

        result.Single(x => x.Id == cachedCurrency.Id)
            .Should().BeEquivalentTo(cachedBeforeUpdate);
        result.Single(x => x.Id == cachedCurrency.Id).Name.Should().NotBe("Cached updated name");
        result.Single(x => x.Id == dbCurrency.Id).Name.Should().Be("Db updated name");
    }

    [Fact]
    public async Task InvalidateCurrency_WhenCurrencyCached_RemovesCurrencyCache()
    {
        var currency = TestContext.Currencies[0];
        var repository = GetRepository();

        await RemoveCachedCurrency(currency.Id);
        await repository.GetCurrency(currency.Id);

        await repository.InvalidateCurrency(currency.Id);
        await UpdateCurrencyName(currency.Id, "Invalidated currency");

        var result = await repository.GetCurrency(currency.Id);

        result!.Name.Should().Be("Invalidated currency");
    }

    [Fact]
    public async Task InvalidateAllCurrencies_WhenCurrenciesCached_RemovesSetAndCurrencyCaches()
    {
        var currencies = TestContext.Currencies;
        var currency = currencies[0];
        var repository = GetRepository();

        await RemoveAllCachedCurrencies(currencies.Select(x => x.Id));
        await repository.GetAllCurrencies();
        await repository.GetCurrency(currency.Id);

        await repository.InvalidateAllCurrencies();
        await UpdateCurrencyName(currency.Id, "Invalidated all currency");

        var result = await repository.GetCurrency(currency.Id);
        var cachedIds = await GetCache().GetFromSetAsync(CacheKeys.CurrencyCache.AllCurrencies());

        result!.Name.Should().Be("Invalidated all currency");
        cachedIds.Should().BeEmpty();
    }

    private ICurrencyCacheRepository GetRepository()
    {
        return Scope.ServiceProvider.GetRequiredService<ICurrencyCacheRepository>();
    }

    private ICache GetCache()
    {
        return Scope.ServiceProvider.GetRequiredService<ICache>();
    }

    private async Task RemoveAllCachedCurrencies(IEnumerable<int> currencyIds)
    {
        await GetCache().RemoveKeyAsync(CacheKeys.CurrencyCache.AllCurrencies());
        await Task.WhenAll(currencyIds.Select(RemoveCachedCurrency));
    }

    private async Task RemoveCachedCurrency(int currencyId)
    {
        await GetCache().RemoveKeyAsync(CacheKeys.CurrencyCache.Currency(currencyId));
    }

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
