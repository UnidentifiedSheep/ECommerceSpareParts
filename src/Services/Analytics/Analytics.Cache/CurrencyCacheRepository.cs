

using Analytics.Application.Interfaces.Cache;
using Analytics.Application.Static;
using Cache;
using Cache.Extensions;
using Internal.Integration.Core.Interfaces.Main;

namespace Analytics.Cache;

public class CurrencyCacheRepository(
    ICache rawCache,
    IMainClient mainClient) : ICurrencyCacheRepository
{
    public Task<decimal?> GetCurrencyRate(int currencyId, CancellationToken cancellationToken = default)
        => rawCache.GetOrSetAsync(
            key: CacheKeys.CurrencyCache.CurrencyRate(currencyId),
            factory: async () =>
            {
                var response = await mainClient.CurrencyNode.GetCurrencyRate(currencyId, cancellationToken);
                return response.Success ? response.ValueOrThrow : (decimal?)null;
            },
            ttl: CacheKeys.CurrencyCache.Ttl);

    public Task InvalidateCurrencyRate(int currencyId, CancellationToken cancellationToken = default)
    {
        return rawCache.RemoveKeyAsync(CacheKeys.CurrencyCache.CurrencyRate(currencyId));
    }
}
