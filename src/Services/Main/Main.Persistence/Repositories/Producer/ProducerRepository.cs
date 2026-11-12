using Main.Entities.Product;
using Persistence.Context;
using EFCore.BulkExtensions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Producer;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Repository;
using QueryExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Main.Persistence.Repositories.Producer;

public class ProducerRepository(UnifiedDbContext context, QueryExtensions extensions)
	: LinqRepositoryBase<UnifiedDbContext, Entities.Producer.Producer, int>(context, extensions), IProducerRepository
{
	public Task<bool> ProducerHasAnyArticle(int producerId, CancellationToken cancellationToken = default) =>
		Context.Set<Main.Entities.Product.Product>().AsNoTracking().AnyAsync(x => x.ProducerId == producerId, cancellationToken);

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
