namespace Search.Application.Interfaces.Producer;

public interface IProducerSearchDocumentProvider
{
    Task<Entities.Producer?> GetById(int producerId, CancellationToken cancellationToken = default);
}
