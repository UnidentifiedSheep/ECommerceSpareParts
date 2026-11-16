using EFCore.BulkExtensions;
using Main.Entities.Product.Producer;
using Main.Modules.Product.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Repository;
using QueryExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Main.Modules.Product.Persistence.Repositories.Producer;

public class ProducerRepository(UnifiedDbContext context, QueryExtensions extensions)
	: LinqRepositoryBase<UnifiedDbContext, Entities.Product.Producer.Producer, int>(context, extensions),
		IProducerRepository
{
	public Task<bool> ProducerHasAnyArticle(int producerId, CancellationToken cancellationToken = default)
		=> Context.Set<Entities.Product.Product.Product>()
			.AsNoTracking()
			.AnyAsync(x => x.ProducerId == producerId, cancellationToken);

	public async Task AddSupplierMappingsOnConflictDoNothingAsync(
		IEnumerable<ProducerSupplierMapping> mappings,
		CancellationToken cancellationToken = default)
	{
		var items = mappings.ToList();
		if (items.Count == 0)
			return;

		await Context.BulkInsertOrUpdateAsync(
			items,
			new BulkConfig
			{
				UpdateByProperties =
				[
					nameof(ProducerSupplierMapping.NormalizedName),
					nameof(ProducerSupplierMapping.Supplier)
				],
				PropertiesToIncludeOnUpdate = [""]
			},
			cancellationToken: cancellationToken);
	}
}
