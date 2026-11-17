using Application.Common.Interfaces.Cache;
using Cache.Extensions;
using Main.Modules.Product.Application.Interfaces.Cache;
using Main.Modules.Product.Application.Static;

namespace Main.Modules.Product.Application.Cache;

public sealed class ProductCacheInvalidator(ICache rawCache) : IProductCacheInvalidator
{
	public Task InvalidateProductAsync(int productId)
		=> rawCache.RemoveKeyAsync(ProductCacheKeys.Product(productId));

	public Task InvalidateProductsAsync(IEnumerable<int> productIds)
		=> rawCache.RemoveKeysAsync(productIds.Select(ProductCacheKeys.Product));

	public Task InvalidateCrossesAsync(int productId)
	{
		var relationKey = ProductCacheKeys.ProductCrossRelations(productId);
		return rawCache.InvalidateByRelationsAsync(relationKey);
	}

	public async Task InvalidateCrossesAsync(IEnumerable<int> productIds)
	{
		var keys = productIds.Select(ProductCacheKeys.ProductCrossRelations).ToList();

		if (keys.Count != 0)
			await rawCache.InvalidateByRelationsAsync(keys);
	}
}
