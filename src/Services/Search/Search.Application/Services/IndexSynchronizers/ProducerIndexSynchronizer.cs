using Search.Application.Interfaces;
using Search.Application.Interfaces.Producer;
using Search.Entities;

namespace Search.Application.Services.IndexSynchronizers;

public class ProducerIndexSynchronizer(
    IProducerSearchDocumentProvider producerSearchDocumentProvider,
    IProducerRepository producerRepository
) : IIndexSynchronizer<Producer, int>
{
    public async Task Reindex(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var producers = await producerSearchDocumentProvider
            .GetByIds(ids, cancellationToken);

        var toDelete = new List<int>();
        var toUpsert = new List<Producer>();

        foreach (var (id, producer) in producers)
        {
            if (producer == null)
                toDelete.Add(id);
            else
                toUpsert.Add(producer);
        }

        await producerRepository.DeleteMany(toDelete, cancellationToken);
        await producerRepository.UpsertMany(toUpsert, cancellationToken);
    }

    public Task Delete(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        return producerRepository.DeleteMany(ids, cancellationToken);
    }
}