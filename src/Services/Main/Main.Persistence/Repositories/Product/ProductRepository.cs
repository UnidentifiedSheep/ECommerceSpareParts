using Application.Common.Interfaces.Repositories;
using EFCore.BulkExtensions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Product;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Repository;

using QueryExtensions = Persistence.Interfaces.IQueryableExtensions;
using ProductEntity = Main.Entities.Product.Product;

namespace Main.Persistence.Repositories.Product;

internal class ProductRepository(DContext context, BulkProductContext bulkContext, QueryExtensions extensions)
	: LinqRepositoryBase<DContext, ProductEntity, int>(context, extensions), IProductRepository
{
	public async Task<IReadOnlyList<ProductEntity>> GetProductCrosses(
		int productId,
		Criteria<Entities.Product.Product> criteria,
		CancellationToken cancellationToken = default)
	{
		var left = Context
			.ProductCrosses
			.Where(c => c.RightProductId == productId)
			.Select(c => c.LeftProduct);

		var right = Context
			.ProductCrosses
			.Where(c => c.LeftProductId == productId)
			.Select(c => c.RightProduct);

		return await QueryableExtensions.Apply(left.Union(right), criteria).ToListAsync(cancellationToken);
	}

	public async Task<HashSet<(string NormalizedSku, int ProducerId)>> GetExistingProductKeys(
		IEnumerable<(string NormalizedSku, int ProducerId)> keys,
		CancellationToken cancellationToken = default)
	{
		var requestedKeys = keys.Distinct().ToHashSet();

		if (requestedKeys.Count == 0)
			return [];

		var normalizedSkus = requestedKeys.Select(x => x.NormalizedSku).Distinct().ToList();

		var producerIds = requestedKeys.Select(x => x.ProducerId).Distinct().ToList();

		var existing = await Context
			.Products
			.AsNoTracking()
			.Where(x => normalizedSkus.Contains(x.Sku.NormalizedValue))
			.Where(x => producerIds.Contains(x.ProducerId))
			.Select(x => new
			{
				x.Sku.NormalizedValue, x.ProducerId
			})
			.ToListAsync(cancellationToken);

		return existing
			.Select(x => (x.NormalizedValue, x.ProducerId))
			.Where(requestedKeys.Contains)
			.ToHashSet();
	}

	public async Task<Dictionary<(string NormalizedSku, int ProducerId), int>> GetProductIdsByKeys(
		IEnumerable<(string NormalizedSku, int ProducerId)> keys,
		CancellationToken cancellationToken = default)
	{
		var requestedKeys = keys.Distinct().ToHashSet();

		if (requestedKeys.Count == 0)
			return [];

		var normalizedSkus = requestedKeys.Select(x => x.NormalizedSku).Distinct().ToList();

		var producerIds = requestedKeys.Select(x => x.ProducerId).Distinct().ToList();

		var products = await Context
			.Products
			.AsNoTracking()
			.Where(x => normalizedSkus.Contains(x.Sku.NormalizedValue))
			.Where(x => producerIds.Contains(x.ProducerId))
			.Select(x => new
			{
				x.Id,
				x.Sku.NormalizedValue,
				x.ProducerId
			})
			.ToListAsync(cancellationToken);

		return products
			.Where(x => requestedKeys.Contains((x.NormalizedValue, x.ProducerId)))
			.ToDictionary(x => (x.NormalizedValue, x.ProducerId), x => x.Id);
	}

	public Task UpsertProductCrosses(
		IEnumerable<ProductCross> crosses,
		CancellationToken cancellationToken = default)
	{
		return Context.BulkInsertOrUpdateAsync(
			crosses,
			new BulkConfig
			{
				UpdateByProperties = [nameof(ProductCross.LeftProductId), nameof(ProductCross.RightProductId)]
			},
			cancellationToken: cancellationToken);
	}

	public async Task InsertProductsOnConflictDoNothingAsync(
		IReadOnlyCollection<ProductEntity> products,
		CancellationToken cancellation = default)
	{
		if (products.Count == 0) return;

		var items = products
			.Select(BulkProduct.FromProduct)
			.DistinctBy(x => (x.NormalizedSku, x.ProducerId))
			.ToList();

		var ambientTransaction = Context.Database.CurrentTransaction
			?? throw new InvalidOperationException("Product bulk insert requires an active transaction.");
		await bulkContext.Database.UseTransactionAsync(ambientTransaction.GetDbTransaction(), cancellation);

		try
		{
			await bulkContext.BulkInsertOrUpdateAsync(
				items,
				new BulkConfig
				{
					UpdateByProperties = [nameof(BulkProduct.NormalizedSku), nameof(BulkProduct.ProducerId)],
					PropertiesToIncludeOnUpdate = [""]
				},
				cancellationToken: cancellation);
		}
		finally
		{
			await bulkContext.Database.UseTransactionAsync(null, CancellationToken.None);
		}
	}
}
