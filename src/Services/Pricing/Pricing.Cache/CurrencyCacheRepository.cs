using Internal.Integration.Core.Interfaces.Main;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Static;
using ZiggyCreatures.Caching.Fusion;

namespace Pricing.Cache;

public class CurrencyCacheRepository(
    IFusionCache fusionCache,
    IMainClient mainClient) : ICurrencyCacheRepository
{
    public async Task<decimal?> GetCurrencyRate(int currencyId, CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.CurrencyCache.CurrencyRate(currencyId);
        var cached = await fusionCache.TryGetAsync<decimal>(key, token: cancellationToken);
        if (cached.HasValue) return cached.Value;
        
        var response = await mainClient.CurrencyNode.GetCurrencyRate(currencyId, cancellationToken);
        decimal? value = response.Success ? response.ValueOrThrow : null;

        if (value == null) return null;
        await fusionCache.SetAsync(key, value, CacheKeys.CurrencyCache.Ttl, token: cancellationToken);
        return value;
    }

    public async Task InvalidateCurrencyRate(int currencyId, CancellationToken cancellationToken = default)
    {
        await fusionCache.RemoveAsync(
            CacheKeys.CurrencyCache.CurrencyRate(currencyId), 
            token: cancellationToken);
    }
}
