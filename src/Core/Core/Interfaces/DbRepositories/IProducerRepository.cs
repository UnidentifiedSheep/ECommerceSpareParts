using Core.Entities;

namespace Core.Interfaces.DbRepositories;

public interface IProducerRepository
{
    Task<IEnumerable<int>> ProducersExistsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    Task<Producer?> GetProducer(int producerId, bool track = true, CancellationToken cancellationToken = default);
    Task<bool> ProducerHasAnyArticle(int producerId, CancellationToken cancellationToken = default);

    Task<bool> OtherNameIsTaken(string otherName, int? producerId = null, string? whereUsed = null,
        CancellationToken cancellationToken = default);

    Task<bool> IsProducerNameTaken(string producerName, CancellationToken cancellationToken = default);

    Task<ProducersOtherName?> GetOtherName(int producerId, string otherName, string? whereUsed, bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Producer>> GetProducers(string? searchTerm, int page, int viewCount, bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ProducersOtherName>> GetOtherNames(int producerId, int page, int viewCount, bool track = true,
        CancellationToken cancellationToken = default);
}