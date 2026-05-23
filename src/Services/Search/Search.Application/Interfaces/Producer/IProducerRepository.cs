using Abstractions.Models;
using Search.Entities;

namespace Search.Application.Interfaces;

public interface IProducerRepository
{
    Task Upsert(Producer producer, CancellationToken token = default);
    Task UpsertMany(IEnumerable<Producer> producers, CancellationToken token = default);
    Task<Producer?> GetById(int id, CancellationToken token = default);
    Task<IReadOnlyCollection<Producer>> Search(
        string query,
        Pagination? pagination = null,
        CancellationToken token = default);
    Task Delete(int id, CancellationToken token = default);
    Task DeleteMany(IEnumerable<int> ids, CancellationToken token = default);
}
