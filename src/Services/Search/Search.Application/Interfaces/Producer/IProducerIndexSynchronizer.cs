namespace Search.Application.Interfaces.Producer;

public interface IProducerIndexSynchronizer
{
    Task Reindex(int producerId, CancellationToken cancellationToken = default);

    Task Delete(int producerId, CancellationToken cancellationToken = default);
}
