using Application.Common.Interfaces.Repositories;
using EFCore.BulkExtensions;
using Main.Application.Interfaces.Persistence;
using Main.Application.Models.Product;
using Main.Entities.Product;
using Main.Enums.Products;
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
	public Task<ProductDeletionBlocker?> GetDeletionBlockerAsync(int productId, CancellationToken cancellationToken = default)
		=> Context.Products
			.AsNoTracking()
			.Where(product => product.Id == productId)
			.Select(product => product.Stock.Value != 0 ? (ProductDeletionBlocker?)ProductDeletionBlocker.Stock
				: Context.StorageContents.Any(item => item.ProductId == productId) ? ProductDeletionBlocker.StorageContent
				: Context.SaleContents.Any(item => item.ProductId == productId) ? ProductDeletionBlocker.Sale
				: Context.PurchaseContents.Any(item => item.ProductId == productId) ? ProductDeletionBlocker.Purchase
				: Context.OrderItems.Any(item => item.ProductId == productId) ? ProductDeletionBlocker.Order
				: Context.ProductReservations.Any(item => item.ProductId == productId) ? ProductDeletionBlocker.Reservation
				: Context.ProductContents.Any(item => item.ChildProductId == productId) ? ProductDeletionBlocker.Composition
				: null)
			.SingleAsync(cancellationToken);

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

	public Task InsertProductCrossesOnConflictDoNothingAsync(
		IEnumerable<ProductCross> crosses,
		CancellationToken cancellationToken = default)
	{
		var items = crosses
			.DistinctBy(x => (x.LeftProductId, x.RightProductId))
			.ToList();

		if (items.Count == 0) return Task.CompletedTask;

		return Context.BulkInsertOrUpdateAsync(
			items,
			new BulkConfig { PropertiesToIncludeOnUpdate = [""] },
			cancellationToken: cancellationToken);
	}

	public Task DeleteProductCrossesAsync(
		IEnumerable<ProductCross> crosses,
		CancellationToken cancellationToken = default)
	{
		var items = crosses
			.DistinctBy(x => (x.LeftProductId, x.RightProductId))
			.ToList();

		return items.Count == 0
			? Task.CompletedTask
			: Context.BulkDeleteAsync(items, cancellationToken: cancellationToken);
	}

	public async Task<IReadOnlyList<int>> InsertProductsOnConflictDoNothingAsync(
		IReadOnlyCollection<ProductEntity> products,
		CancellationToken cancellation = default)
	{
		if (products.Count == 0) return [];

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
					PropertiesToIncludeOnUpdate = [""],
					SetOutputIdentity = true
				},
				cancellationToken: cancellation);

			return items.Where(item => item.Id != 0).Select(item => item.Id).ToArray();
		}
		finally
		{
			await bulkContext.Database.UseTransactionAsync(null, CancellationToken.None);
		}
	}

	public Task InsertProductEansOnConflictDoNothingAsync(
		IReadOnlyCollection<ProductEan> eans,
		CancellationToken cancellationToken = default)
	{
		if (eans.Count == 0) return Task.CompletedTask;

		var items = eans.DistinctBy(x => (x.ProductId, x.Ean)).ToList();
		return Context.BulkInsertOrUpdateAsync(
			items,
			new BulkConfig { PropertiesToIncludeOnUpdate = [""] },
			cancellationToken: cancellationToken);
	}

	public Task InsertProductImagesOnConflictDoNothingAsync(
		IReadOnlyCollection<ProductImage> images,
		CancellationToken cancellationToken = default)
	{
		if (images.Count == 0) return Task.CompletedTask;

		var items = images.DistinctBy(image => image.GetId()).ToList();
		return Context.BulkInsertOrUpdateAsync(
			items,
			new BulkConfig { PropertiesToIncludeOnUpdate = [""] },
			cancellationToken: cancellationToken);
	}

	public Task UpsertProductCharacteristicsAsync(
		IReadOnlyCollection<ProductCharacteristic> characteristics,
		CancellationToken cancellationToken = default)
	{
		var items = characteristics.GroupBy(characteristic => characteristic.GetId())
			.Select(group => group.Last())
			.ToList();
		if (items.Count == 0) return Task.CompletedTask;

		return Context.BulkInsertOrUpdateAsync(
			items,
			new BulkConfig { PropertiesToIncludeOnUpdate = [nameof(ProductCharacteristic.Value)] },
			cancellationToken: cancellationToken);
	}

	public Task UpsertProductContentsAsync(
		IReadOnlyCollection<ProductContent> contents,
		CancellationToken cancellationToken = default)
	{
		var items = contents.GroupBy(content => content.GetId())
			.Select(group => group.Last())
			.ToList();
		if (items.Count == 0) return Task.CompletedTask;

		return Context.BulkInsertOrUpdateAsync(
			items,
			new BulkConfig { PropertiesToIncludeOnUpdate = [nameof(ProductContent.Quantity)] },
			cancellationToken: cancellationToken);
	}

	public async Task<IReadOnlyList<int>> UpdateProductGroupsAsync(
		IReadOnlyCollection<ProductGroupAssignment> assignments,
		CancellationToken cancellationToken = default)
	{
		if (assignments.Count == 0) return [];

		var items = assignments.Select(x => new BulkProduct
		{
			Sku = string.Empty,
			NormalizedSku = x.NormalizedSku,
			Name = string.Empty,
			ProducerId = x.ProducerId,
			ProductGroupId = x.ProductGroupId
		}).ToList();

		var ambientTransaction = Context.Database.CurrentTransaction
			?? throw new InvalidOperationException("Product group bulk update requires an active transaction.");
		await bulkContext.Database.UseTransactionAsync(ambientTransaction.GetDbTransaction(), cancellationToken);

		try
		{
			await bulkContext.BulkUpdateAsync(
				items,
				new BulkConfig
				{
					PropertiesToIncludeOnUpdate = [nameof(BulkProduct.ProductGroupId)],
					SetOutputIdentity = true
				},
				cancellationToken: cancellationToken);

			return items.Where(item => item.Id != 0).Select(item => item.Id).ToArray();
		}
		finally
		{
			await bulkContext.Database.UseTransactionAsync(null, CancellationToken.None);
		}
	}
}
