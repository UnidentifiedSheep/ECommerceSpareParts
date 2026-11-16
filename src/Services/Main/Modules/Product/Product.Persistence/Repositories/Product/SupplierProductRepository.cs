using EFCore.BulkExtensions;
using Enums;
using Main.Entities.Product.Product.Enrichment;
using Main.Modules.Product.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Repository;
using QueryExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Main.Modules.Product.Persistence.Repositories.Product;

public class SupplierProductRepository(UnifiedDbContext context, QueryExtensions extensions)
	: LinqRepositoryBase<UnifiedDbContext, SupplierProduct, int>(context, extensions),
		ISupplierProductRepository
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
			.Set<SupplierProduct>()
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
