using Cache;
using Cache.Extensions;
using Integrations.Supplier;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Static;
using Pricing.Enums;

namespace Pricing.Cache;

public class PricingCacheRepository(
    ICache cache
    ) : IPricingCacheRepository
{
    public async Task<string> TryLockSupplierRequestAsync(
        int productId,
        Supplier supplier,
        string lockKey,
        CancellationToken cancellationToken = default)
    {
        return await cache.GetOrSetAsync(
            key: CacheKeys.PricingCache.Lock.SupplierRequest(productId, supplier),
            factory: () => Task.FromResult<string?>(lockKey),
            CacheKeys.PricingCache.Lock.Ttl) ?? lockKey;
    }
}