using Cache;
using Cache.Extensions;
using Enums;
using Integrations.Supplier;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Static;

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
            CacheKeys.PricingCache.Lock.SupplierRequest(productId, supplier),
            () => Task.FromResult<string?>(lockKey),
            CacheKeys.PricingCache.Lock.Ttl) ?? lockKey;
    }
}