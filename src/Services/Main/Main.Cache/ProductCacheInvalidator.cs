using Application.Common.Interfaces.Cache;
using Cache.Extensions;
using Main.Application.Interfaces.Cache;
using Main.Application.Static;

namespace Main.Cache;

public sealed class ProductCacheInvalidator(ICache rawCache) : IProductCacheInvalidator
{
	public Task InvalidateProductAsync(int productId) =>
		rawCache.RemoveKeyAsync(CacheKeys.ProductCache.Product(productId));

	public Task InvalidateProductsAsync(IEnumerable<int> productIds) =>
		rawCache.RemoveKeysAsync(productIds.Select(CacheKeys.ProductCache.Product));

	public Task InvalidateCrossesAsync(int productId)
	{
		var relationKey = CacheKeys.ProductCache.ProductCrossRelations(productId);
		return rawCache.InvalidateByRelationsAsync(relationKey);
	}

	public async Task InvalidateCrossesAsync(IEnumerable<int> productIds)
	{
		var keys = productIds.Select(CacheKeys.ProductCache.ProductCrossRelations).ToList();

		if (keys.Count != 0)
			await rawCache.InvalidateByRelationsAsync(keys);
	}
}
