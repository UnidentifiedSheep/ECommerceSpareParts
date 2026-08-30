using Application.Common.Extensions;
using Application.Common.Interfaces.Cache;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Cache.Extensions;
using Main.Application.Dtos.Product;
using Main.Application.Extensions.QueryExtensions;
using Main.Application.Interfaces.Products;
using Main.Application.Models.Product;
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
		string[]? sortBy,
		CancellationToken cancellationToken = default)
	{
		var request = new ProductCrossesRequestItem(productId, sortBy);
		var result = await GetProductsCrossesAsync([request], cancellationToken);

		return result[request];
	}

	public async Task<IReadOnlyDictionary<ProductCrossesRequestItem, IReadOnlyList<int>>>
		GetProductsCrossesAsync(
			IEnumerable<ProductCrossesRequestItem> requests,
			CancellationToken cancellationToken = default)
	{
		var uniqueRequests = requests.Distinct().ToArray();
		var result = new Dictionary<ProductCrossesRequestItem, IReadOnlyList<int>>(uniqueRequests.Length);

		if (uniqueRequests.Length == 0)
			return result;

		var cacheKeys = uniqueRequests.Select(GetCrossesCacheKey).ToArray();
		var cachedValues = await rawCache.GetAsync<int[]>(cacheKeys);
		var missingRequests = new List<ProductCrossesRequestItem>();

		for (var i = 0; i < uniqueRequests.Length; i++)
		{
			var cached = cachedValues[i];
			if (cached is null)
				missingRequests.Add(uniqueRequests[i]);
			else
				result[uniqueRequests[i]] = cached;
		}

		if (missingRequests.Count == 0)
			return result;

		var loaded = await GetCrossesFromDb(missingRequests, cancellationToken);

		await rawCache.SetAsync(
			missingRequests.Select(request => (GetCrossesCacheKey(request), loaded[request].ToArray())),
			CacheKeys.ProductCache.Ttl);

		var relations = missingRequests
			.SelectMany(request => loaded[request]
				.Prepend(request.ProductId)
				.Distinct()
				.Select(productId => new
				{
					RelationKey = CacheKeys.ProductCache.ProductCrossRelations(productId),
					CacheKey = GetCrossesCacheKey(request)
				}))
			.GroupBy(x => x.RelationKey)
			.ToDictionary(group => group.Key, group => group.Select(x => x.CacheKey).Distinct().ToList());

		await rawCache.AddToSetAsync(relations, CacheKeys.ProductCache.Ttl);

		foreach (var item in loaded)
			result[item.Key] = item.Value;

		return result;
	}

	public async Task<IReadOnlyList<ProductDto?>> GetProductsAsync(IEnumerable<int> ids) =>
		await rawCache.GetAsync<ProductDto>(ids.Select(CacheKeys.ProductCache.Product));

	private static string GetCrossesCacheKey(ProductCrossesRequestItem request) =>
		CacheKeys.ProductCache.ProductCrosses(request.ProductId, request.SortBy);

	private async Task<Dictionary<ProductCrossesRequestItem, IReadOnlyList<int>>> GetCrossesFromDb(
		IReadOnlyCollection<ProductCrossesRequestItem> requests,
		CancellationToken cancellationToken)
	{
		var productIds = requests.Select(x => x.ProductId).ToHashSet();
		var result = requests.ToDictionary(request => request, _ => (IReadOnlyList<int>)[]);

		var crosses = await crossesReadRepository.Query.GetCrosses(productIds).ToListAsync(cancellationToken);
		var crossesByProductId = crosses.GetCrosses(productIds);

		var groups = requests.GroupBy(request => request.SortBy, SortByComparer.Instance);

		foreach (var group in groups)
		{
			var crossProductIds = group
				.SelectMany(request => crossesByProductId[request.ProductId])
				.ToHashSet();

			if (crossProductIds.Count == 0)
				continue;

			var orderedCrossProductIds = await productReadRepository
				.Query
				.Where(x => crossProductIds.Contains(x.Id))
				.SortBy(group.Key.ToArray())
				.Select(x => x.Id)
				.ToListAsync(cancellationToken);

			foreach (var request in group)
			{
				var requestCrosses = crossesByProductId[request.ProductId];
				result[request] = orderedCrossProductIds.Where(requestCrosses.Contains).ToArray();
			}
		}

		return result;
	}

	private async Task<ProductDto> GetProductFromDb(int id, CancellationToken cancellationToken)
	{
		return await productReadRepository.Query.FirstProductDtoAsync(
			productProjection,
			x => x.Id == id,
			cancellationToken) ?? throw new ProductNotFoundException(id);
	}

	private sealed class SortByComparer : IEqualityComparer<IReadOnlyList<string>>
	{
		public static readonly SortByComparer Instance = new();

		public bool Equals(IReadOnlyList<string>? x, IReadOnlyList<string>? y)
		{
			return ReferenceEquals(x, y) ||
				x is not null && y is not null && x.SequenceEqual(y, StringComparer.Ordinal);
		}

		public int GetHashCode(IReadOnlyList<string> value)
		{
			var hash = new HashCode();
			foreach (var item in value)
				hash.Add(item, StringComparer.Ordinal);
			return hash.ToHashCode();
		}
	}
}
