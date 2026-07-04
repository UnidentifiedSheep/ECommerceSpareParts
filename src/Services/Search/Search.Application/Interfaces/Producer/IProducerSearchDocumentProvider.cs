namespace Search.Application.Interfaces.Producer;

public interface IProducerSearchDocumentProvider
{
    Task<Dictionary<int, Entities.Producer?>> GetByIds(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default);
}