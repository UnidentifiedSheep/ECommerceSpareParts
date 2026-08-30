using EFCore.BulkExtensions;
using Enums;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Product.Enrichment;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Repository;
using QueryExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Main.Persistence.Repositories.Product;

public class SupplierProductRepository(DContext context, QueryExtensions extensions)
	: LinqRepositoryBase<DContext, SupplierProduct, int>(context, extensions), ISupplierProductRepository
{
	public async Task<IReadOnlyList<SupplierProduct>> GetBySupplierKeysAsync(
		Supplier supplier,
		IEnumerable<(string NormalizedSku, string Producer)> keys,
		CancellationToken cancellationToken = default)
	{
		var requestedKeys = keys.Distinct().ToHashSet();

		if (requestedKeys.Count == 0)
			return [];

		var normalizedSkus = requestedKeys.Select(x => x.NormalizedSku).Distinct().ToList();
		var producers = requestedKeys.Select(x => x.Producer).Distinct().ToList();

		var products = await Context
			.SupplierProducts
			.Include(x => x.Names)
			.Where(x => x.Supplier == supplier)
			.Where(x => normalizedSkus.Contains(x.Sku.NormalizedValue))
			.Where(x => producers.Contains(x.Producer))
			.ToListAsync(cancellationToken);

		return products.Where(x => requestedKeys.Contains((x.Sku.NormalizedValue, x.Producer))).ToList();
	}

	public async Task UpsertCrossesAsync(
		IEnumerable<SupplierProductCross> crosses,
		CancellationToken cancellationToken = default)
	{
		var items = crosses.ToList();
		if (items.Count == 0)
			return;

		await Context.BulkInsertOrUpdateAsync(
			items,
			new BulkConfig
			{
				UpdateByProperties =
				[
					nameof(SupplierProductCross.LeftId), nameof(SupplierProductCross.RightId)
				]
			},
			cancellationToken: cancellationToken);
	}
}
