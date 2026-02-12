using Abstractions.Interfaces.Cache;
using Pricing.Abstractions.Interfaces.CacheRepositories;
using Pricing.Abstractions.Models;

namespace Pricing.Cache.Repositories;

public class CurrencyCacheRepository : ICurrencyCacheRepository
{
    private const string CurrenciesKey = "currencies";
    private readonly ICache _cache;
    private readonly TimeSpan? _ttl;

    public CurrencyCacheRepository(ICache cache, TimeSpan? ttl = null)
    {
        _cache = cache;
        _ttl = ttl;
    }
    
    public async Task<List<Currency>?> GetCurrencies()
    {
        return await _cache.StringGetAsync<List<Currency>>(CurrenciesKey);
    }
    
    public async Task SetCurrencies(List<Currency> currencies)
    {
        await _cache.StringSetAsync(CurrenciesKey, currencies, _ttl);
    }
    
    public async Task DeleteCurrencies()
    {
        await _cache.DeleteAsync(CurrenciesKey);
    }
}