using Abstractions.Models;

namespace Search.Application.Interfaces.Producer;

public interface IProducerRepository
{
    Task Upsert(Entities.Producer producer, CancellationToken token = default);
    Task UpsertMany(IEnumerable<Entities.Producer> producers, CancellationToken token = default);
    Task<Entities.Producer?> GetById(int id, CancellationToken token = default);

    Task<IReadOnlyCollection<Entities.Producer>> Search(
        string? query,
        Pagination? pagination = null,
        CancellationToken token = default);

    Task Delete(int id, CancellationToken token = default);
    Task DeleteMany(IEnumerable<int> ids, CancellationToken token = default);
}