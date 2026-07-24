using Application.Common.Extensions;
using Application.Common.Interfaces.Cache;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Cache;
using Cache.Extensions;
using Main.Application.Dtos.Currencies;
using Main.Application.Interfaces.Cache;
using Main.Application.Static;
using Main.Entities.Currency;
using Main.Entities.Settings;
using Microsoft.EntityFrameworkCore;

namespace Main.Cache;

public class CurrencyCacheRepository(
    ICache rawCache,
    ISettingsService settingsService,
    IRepository<CurrencyRate, (int, int)> rateRepository,
    IReadRepository<Currency, int> repository,
    IProjectionProvider<Currency, CurrencyDto> projection
) : ICurrencyCacheRepository
{
    public async Task<CurrencyDto?> GetCurrency(
        int id,
        CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.CurrencyCache.Currency(id);
        return await rawCache.GetOrSetAsync(
            key,
            () => GetCurrencyFromDb(id),
            CacheKeys.CurrencyCache.Ttl);
    }

    public async Task<IReadOnlyList<CurrencyDto>> GetAllCurrencies(
        CancellationToken cancellationToken = default)
    {
        var currenciesKey = CacheKeys.CurrencyCache.AllCurrencies();
        var allCurrencies = await rawCache.GetFromSetAsync(currenciesKey);

        if (allCurrencies.Length != 0)
            return (await rawCache.GetOrSetManyAsync(
                    allCurrencies.Select(int.Parse),
                    CacheKeys.CurrencyCache.Currency,
                    currency => currency.Id,
                    GetMissingCurrenciesFromDb,
                    CacheKeys.CurrencyCache.Ttl))
                .Select(x => x.Value)
                .ToList();

        var currencies = await repository.Query
            .Project(projection)
            .ToListAsync(cancellationToken);

        await rawCache.AddToSetAsync(currenciesKey, currencies.Select(x => x.Id.ToString()));
        return currencies;
    }

    public Task<decimal?> GetCurrencyRate(int currencyId, CancellationToken cancellationToken = default)
    {
        return rawCache.GetOrSetAsync(
            CacheKeys.CurrencyCache.CurrencyRate(currencyId),
            () => GetRateFromDb(currencyId),
            CacheKeys.CurrencyCache.Ttl);
    }

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
    {
        return repository.Query.Where(x => x.Id == id)
            .Project(projection)
            .FirstOrDefaultAsync();
    }

    private Task<Dictionary<int, CurrencyDto>> GetMissingCurrenciesFromDb(IEnumerable<int> ids)
    {
        return repository.Query
            .Where(x => ids.Contains(x.Id))
            .Project(projection)
            .ToDictionaryAsync(x => x.Id);
    }

    private async Task<decimal?> GetRateFromDb(int currencyId)
    {
        var baseCurrencyId = (await settingsService.GetOrDefault<CurrencySetting>())
            .Data
            .BaseCurrencyId;

        if (currencyId == baseCurrencyId) return 1m;

        return (await rateRepository.GetById((currencyId, baseCurrencyId)))?.Rate;
    }
}
