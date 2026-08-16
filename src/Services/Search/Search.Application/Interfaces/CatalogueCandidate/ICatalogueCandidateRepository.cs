namespace Search.Application.Interfaces.CatalogueCandidate;

public interface ICatalogueCandidateRepository
{
    Task UpsertMany(
        IEnumerable<Entities.CatalogueCandidate> candidates,
        CancellationToken cancellationToken = default);

    Task DeleteMany(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default);
}
