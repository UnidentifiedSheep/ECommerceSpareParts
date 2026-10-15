using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Products;
using Main.Entities.Product;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Main.Entities.Product.Product;

namespace Main.Application.Services.Products;

public sealed class ProductEventProvider(
	IReadRepository<Product, int> productRepository,
	IEnumerable<IProductEventPartProvider> partProviders) : IProductEventProvider
{
	public async Task<IReadOnlyList<object>> GetEventsAsync(
		IEnumerable<int> productIds,
		ProductEventParts parts,
		CancellationToken cancellationToken = default)
	{
		var ids = productIds.Distinct().Order().ToList();
		var providers = partProviders
			.Where(provider => parts.HasFlag(provider.Part))
			.ToList();
		if (ids.Count == 0 || providers.Count == 0) return [];

		var updatedAt = DateTime.UtcNow;
		var query = productRepository.Query.Where(product => ids.Contains(product.Id));
		foreach (var provider in providers)
			query = provider.Populate(query);

		query = providers.Any(provider => provider.IsHeavyRequest) ? query.AsSplitQuery() : query.AsSingleQuery();
		var products = await query.OrderBy(product => product.Id).ToListAsync(cancellationToken);
		var found = products.Select(product => product.Id).ToHashSet();
		var missingProductIds = ids.Where(id => !found.Contains(id)).ToArray();
		return providers.Select(provider => provider.Extract(products, updatedAt, missingProductIds)).ToArray();
	}
}
