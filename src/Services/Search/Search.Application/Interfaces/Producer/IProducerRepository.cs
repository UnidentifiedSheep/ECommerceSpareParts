using Abstractions.Models;

namespace Search.Application.Interfaces.Producer;

public interface IProducerRepository
    : ISearchRepository<Entities.Producer, int>
{
    Task<IReadOnlyCollection<Entities.Producer>> Search(
        string? query,
        Pagination? pagination = null,
        CancellationToken token = default);

}
