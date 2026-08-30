using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Producer;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Services;

public class ProducerLookupService(
	IReadRepository<Producer, int> producerReadRepository,
	IReadRepository<ProducerSupplierMapping, int> mappingRepository) : IProducerLookupService
{
	private readonly object _loadLock = new();

	private Task<IProducerLookup>? _loadTask;

	public Task<IProducerLookup> Load(CancellationToken cancellationToken = default)
	{
		Task<IProducerLookup> loadTask;
		lock (_loadLock)
			loadTask = _loadTask ??= LoadCore(cancellationToken);

		return AwaitAndResetOnFailure(loadTask);
	}

	private async Task<IProducerLookup> LoadCore(CancellationToken cancellationToken)
	{
		var producerNamesToIds = new Dictionary<string, int>();
		var aliasesToIds = new Dictionary<string, int>();

		const int batchSize = 1000;

		var baseQuery = producerReadRepository
			.Query
			.Select(x => new
			{
				id = x.Id,
				name = x.Name,
				aliases = x.Aliases.Select(z => z.Alias)
			})
			.OrderBy(x => x.id);

		var lastId = 0;

		while (true)
		{
			var id = lastId;
			var producers = await baseQuery
				.Where(x => x.id > id)
				.Take(batchSize)
				.ToListAsync(cancellationToken);

			if (producers.Count == 0)
				break;

			lastId = producers.Last().id;

			foreach (var item in producers)
			{
				producerNamesToIds.TryAdd(item.name, item.id);
				foreach (var alias in item.aliases)
					aliasesToIds.TryAdd(alias, item.id);
			}

			if (producers.Count != batchSize)
				break;
		}

		var supplierMappingItems = await mappingRepository
			.Query
			.Select(x => new
			{
				x.Supplier,
				x.SupplierProducerName,
				x.ProducerId
			})
			.ToListAsync(cancellationToken);

		var supplierMappings = supplierMappingItems.ToDictionary(
			x => new ProducerSupplierLookupKey(x.Supplier, x.SupplierProducerName),
			x => x.ProducerId);

		IProducerLookup lookup = new ProducerLookup(producerNamesToIds, aliasesToIds);

		return new SupplierProducerLookup(lookup, supplierMappings);
	}

	private async Task<IProducerLookup> AwaitAndResetOnFailure(Task<IProducerLookup> loadTask)
	{
		try
		{
			return await loadTask;
		}
		catch
		{
			lock (_loadLock)
				if (ReferenceEquals(_loadTask, loadTask))
					_loadTask = null;

			throw;
		}
	}
}
