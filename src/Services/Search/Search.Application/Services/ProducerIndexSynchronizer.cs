using Search.Application.Interfaces;
using Search.Application.Interfaces.Producer;

namespace Search.Application.Services;

public class ProducerIndexSynchronizer(
    IProducerSearchDocumentProvider producerSearchDocumentProvider,
    IProducerRepository producerRepository) : IProducerIndexSynchronizer
{
    public async Task Reindex(
        int producerId,
        CancellationToken cancellationToken = default)
    {
        var producer = await producerSearchDocumentProvider.GetById(producerId, cancellationToken);

        if (producer == null)
        {
            await producerRepository.Delete(producerId, cancellationToken);
            return;
        }

        await producerRepository.Upsert(producer, cancellationToken);
    }

    public Task Delete(
        int producerId,
        CancellationToken cancellationToken = default)
    {
        return producerRepository.Delete(producerId, cancellationToken);
    }
}
