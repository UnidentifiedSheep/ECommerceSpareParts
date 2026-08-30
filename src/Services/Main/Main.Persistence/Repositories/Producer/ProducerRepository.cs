using EFCore.BulkExtensions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Producer;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Repository;
using QueryExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Main.Persistence.Repositories.Producer;

public class ProducerRepository(DContext context, QueryExtensions extensions)
	: LinqRepositoryBase<DContext, Entities.Producer.Producer, int>(context, extensions), IProducerRepository
{
	public Task<bool> ProducerHasAnyArticle(int producerId, CancellationToken cancellationToken = default)
	{
		return Context.Products.AsNoTracking().AnyAsync(x => x.ProducerId == producerId, cancellationToken);
	}

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
					nameof(ProducerSupplierMapping.SupplierProducerName),
					nameof(ProducerSupplierMapping.Supplier)
				],
				PropertiesToIncludeOnUpdate = [""]
			},
			cancellationToken: cancellationToken);
	}
}
