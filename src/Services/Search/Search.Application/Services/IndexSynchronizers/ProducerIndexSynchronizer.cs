using Search.Application.Interfaces;
using Search.Application.Interfaces.Producer;
using Search.Entities;

namespace Search.Application.Services.IndexSynchronizers;

public class ProducerIndexSynchronizer(
    IProducerSearchDocumentProvider producerSearchDocumentProvider,
    IProducerRepository producerRepository) : IIndexSynchronizer<Producer, int>
{
    public async Task Reindex(
        int id,
        CancellationToken cancellationToken = default)
    {
        var producer = await producerSearchDocumentProvider.GetById(id, cancellationToken);

        if (producer == null)
        {
            await producerRepository.Delete(id, cancellationToken);
            return;
        }

        await producerRepository.Upsert(producer, cancellationToken);
    }

    public Task Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        return producerRepository.Delete(id, cancellationToken);
    }
}
