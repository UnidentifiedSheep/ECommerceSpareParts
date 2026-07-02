using Integrations.Supplier;
using Pricing.Enums;

namespace Pricing.Application.Interfaces.Cache;

public interface IPricingCacheRepository
{
    /// <summary>
    /// Sets lock record in a cache for a specified product and supplier, to signalize that
    /// we are trying to get prices from this supplier and product.
    /// </summary>
    /// <param name="productId">ID of the product</param>
    /// <param name="supplier">Supplier enum definition</param>
    /// <param name="lockKey">Your key for lock</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    /// <returns>When no lock is currently set returns your <c>lockKey</c>,
    ///  else returns current lockKey in cache.</returns>
    Task<string> TryLockSupplierRequestAsync(
        int productId,
        Supplier supplier,
        string lockKey,
        CancellationToken cancellationToken = default);
}