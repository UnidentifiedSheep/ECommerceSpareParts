using Application.Common.Interfaces.Cache;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Cache.Extensions;
using Main.Application.Dtos.Product;
using Main.Application.Extensions.QueryExtensions;
using Main.Application.Products.Interfaces;
using Main.Application.Static;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Cache;

public class ProductProvider(
	ICache rawCache,
	IReadRepository<ProductCross, (int, int)> crossesReadRepository,
	IReadRepository<Product, int> productReadRepository,
	IProjectionProvider<Product, ProductDto> productProjection) : IProductProvider
{
	public async Task<ProductDto> GetProductOrSetAsync(
		int productId,
		CancellationToken cancellationToken = default)
	{
		var key = CacheKeys.ProductCache.Product(productId);
		var cached = await rawCache.GetAsync<ProductDto>(key);

		if (cached != null)
			return cached;

		var product = await GetProductFromDb(productId, cancellationToken);

		await rawCache.SetAsync([(key, product)], CacheKeys.ProductCache.Ttl);

		return product;
	}

	public async Task<ProductDto?> GetProductAsync(
		int productId,
		CancellationToken cancellationToken = default) =>
		await rawCache.GetAsync<ProductDto>(CacheKeys.ProductCache.Product(productId));

	public async Task<Dictionary<int, ProductDto>> GetProductsOrSetAsync(
		IEnumerable<int> ids,
		CancellationToken cancellationToken = default)
	{
		return await rawCache.GetOrSetManyAsync(
			ids,
			CacheKeys.ProductCache.Product,
			product => product.Id,
			missingIds => productReadRepository.Query.DictionaryProductDto(
				productProjection,
				x => missingIds.Contains(x.Id),
				cancellationToken),
			CacheKeys.ProductCache.Ttl);
	}

	public async Task<IReadOnlyList<int>> GetProductCrossesAsync(
		int productId,
		CancellationToken cancellationToken = default)
	{
		var result = await GetProductsCrossesAsync([productId], cancellationToken);
		return result[productId];
	}

	public async Task<IReadOnlyDictionary<int, IReadOnlyList<int>>> GetProductsCrossesAsync(
		IEnumerable<int> productIds,
		CancellationToken cancellationToken = default)
	{
		var ids = productIds.Distinct().ToArray();
		var result = new Dictionary<int, IReadOnlyList<int>>(ids.Length);
		if (ids.Length == 0) return result;

		var cachedValues = await rawCache.GetAsync<int[]>(ids.Select(CacheKeys.ProductCache.ProductCrosses));
		var missingIds = new List<int>();
		for (var i = 0; i < ids.Length; i++)
		{
			if (cachedValues[i] is { } cached)
				result[ids[i]] = cached;
			else
				missingIds.Add(ids[i]);
		}
		if (missingIds.Count == 0) return result;

		var missingIdSet = missingIds.ToHashSet();
		var crosses = await crossesReadRepository.Query.GetCrosses(missingIdSet).ToListAsync(cancellationToken);
		var loaded = crosses.GetCrosses(missingIdSet);
		await rawCache.SetAsync(
			missingIds.Select(id => (CacheKeys.ProductCache.ProductCrosses(id), loaded[id].ToArray())),
			CacheKeys.ProductCache.Ttl);

		var relations = missingIds
			.SelectMany(id => loaded[id].Prepend(id).Distinct().Select(productId => new
			{
				RelationKey = CacheKeys.ProductCache.ProductCrossRelations(productId),
				CacheKey = CacheKeys.ProductCache.ProductCrosses(id)
			}))
			.GroupBy(item => item.RelationKey)
			.ToDictionary(group => group.Key, group => group.Select(item => item.CacheKey).Distinct().ToList());
		await rawCache.AddToSetAsync(relations, CacheKeys.ProductCache.Ttl);

		foreach (var id in missingIds)
			result[id] = loaded[id].ToArray();
		return result;
	}

	public async Task<IReadOnlyList<ProductDto?>> GetProductsAsync(IEnumerable<int> ids) =>
		await rawCache.GetAsync<ProductDto>(ids.Select(CacheKeys.ProductCache.Product));

	private async Task<ProductDto> GetProductFromDb(int id, CancellationToken cancellationToken)
	{
		return await productReadRepository.Query.FirstProductDtoAsync(
			productProjection,
			x => x.Id == id,
			cancellationToken) ?? throw new ProductNotFoundException(id);
	}

}
