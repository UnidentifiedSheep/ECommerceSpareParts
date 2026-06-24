using Application.Common.Interfaces.Repositories;
using Cache;
using Cache.Extensions;
using LinqKit;
using Main.Application.Dtos.Currencies;
using Main.Application.Interfaces.Cache;
using Main.Application.Projections;
using Main.Application.Static;
using Main.Entities.Currency;
using Microsoft.EntityFrameworkCore;

namespace Main.Cache;

public class CurrencyCacheRepository(
    ICache rawCache,
    IReadRepository<Currency, int> repository) : ICurrencyCacheRepository
{
    public async Task<CurrencyDto?> GetCurrency(
        int id, 
        CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.CurrencyCache.Currency(id);
        return await rawCache.GetOrSetAsync(key, () => GetCurrencyFromDb(id), CacheKeys.CurrencyCache.Ttl);
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

    public Task InvalidateCurrency(int id, CancellationToken cancellationToken = default)
    {
        return rawCache.RemoveKeyAsync(CacheKeys.CurrencyCache.Currency(id));
    }

    public Task InvalidateAllCurrencies(CancellationToken cancellationToken = default)
    {
        return InvalidateAllCurrenciesCore();
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

    private async Task InvalidateAllCurrenciesCore()
    {
        var currenciesKey = CacheKeys.CurrencyCache.AllCurrencies();
        var currencyIds = await rawCache.GetFromSetAsync(currenciesKey);
        var keys = currencyIds
            .Select(int.Parse)
            .Select(CacheKeys.CurrencyCache.Currency)
            .Append(currenciesKey);

        await rawCache.RemoveKeysAsync(keys);
    }
}
