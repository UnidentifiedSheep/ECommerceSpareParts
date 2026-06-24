using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Cache;
using Cache.Extensions;
using LinqKit;
using Main.Application.Dtos.Currencies;
using Main.Application.Interfaces.Cache;
using Main.Application.Projections;
using Main.Application.Static;
using Main.Entities.Currency;
using Main.Entities.Exceptions;
using Main.Entities.Setting;
using Microsoft.EntityFrameworkCore;

namespace Main.Cache;

public class CurrencyCacheRepository(
    ICache rawCache,
    ISettingsService settingsService,
    IRepository<CurrencyRate, (int, int)> rateRepository,
    IReadRepository<Currency, int> repository) : ICurrencyCacheRepository
{
    public async Task<CurrencyDto?> GetCurrency(
        int id, 
        CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.CurrencyCache.Currency(id);
        return await rawCache.GetOrSetAsync(
            key: key, 
            factory: () => GetCurrencyFromDb(id), 
            ttl: CacheKeys.CurrencyCache.Ttl);
    }

    public async Task<IReadOnlyList<CurrencyDto>> GetAllCurrencies(CancellationToken cancellationToken = default)
    {
        var currenciesKey = CacheKeys.CurrencyCache.AllCurrencies();
        var allCurrencies = await rawCache.GetFromSetAsync(currenciesKey);

        if (allCurrencies.Length != 0)
            return (await rawCache.GetOrSetManyAsync(
                ids: allCurrencies.Select(int.Parse),
                CacheKeys.CurrencyCache.Currency,
                currency => currency.Id,
                GetMissingCurrenciesFromDb,
                CacheKeys.CurrencyCache.Ttl))
                .Select(x => x.Value)
                .ToList();
        
        var currencies = await repository.Query
            .AsExpandable()
            .Select(CurrencyProjections.ToDto)
            .ToListAsync(cancellationToken);

        await rawCache.AddToSetAsync(currenciesKey, currencies.Select(x => x.Id.ToString()));
        return currencies;
    }

    public Task<decimal?> GetCurrencyRate(int currencyId, CancellationToken cancellationToken = default)
        => rawCache.GetOrSetAsync(
            key: CacheKeys.CurrencyCache.CurrencyRate(currencyId),
            factory: () => GetRateFromDb(currencyId),
            ttl: CacheKeys.CurrencyCache.Ttl);

    public Task InvalidateCurrency(int id, CancellationToken cancellationToken = default)
    {
        return rawCache.RemoveKeyAsync(CacheKeys.CurrencyCache.Currency(id));
    }

    public async Task InvalidateAllCurrencies(CancellationToken cancellationToken = default)
    {
        var currenciesKey = CacheKeys.CurrencyCache.AllCurrencies();
        var currencyIds = await rawCache.GetFromSetAsync(currenciesKey);
        
        var keys = currencyIds
            .Select(int.Parse)
            .Select(CacheKeys.CurrencyCache.Currency)
            .Append(currenciesKey);

        await rawCache.RemoveKeysAsync(keys);
    }

    public Task InvalidateCurrencyRate(int currencyId, CancellationToken cancellationToken = default)
    {
        return rawCache.RemoveKeyAsync(CacheKeys.CurrencyCache.CurrencyRate(currencyId));
    }

    private Task<CurrencyDto?> GetCurrencyFromDb(int id)
        => repository.Query.Where(x => x.Id == id)
            .AsExpandable().Select(CurrencyProjections.ToDto).FirstOrDefaultAsync();

    private Task<Dictionary<int, CurrencyDto>> GetMissingCurrenciesFromDb(IEnumerable<int> ids)
        => repository.Query
            .Where(x => ids.Contains(x.Id))
            .AsExpandable()
            .Select(CurrencyProjections.ToDto)
            .ToDictionaryAsync(x => x.Id);

    private async Task<decimal?> GetRateFromDb(int currencyId)
    {
        var baseCurrencyId = (await settingsService.GetOrDefault<CurrencySetting>())
            .Data
            .BaseCurrencyId;

        if (currencyId == baseCurrencyId) return 1m;

        return (await rateRepository.GetById((currencyId, baseCurrencyId)))?.Rate;
    }
}
