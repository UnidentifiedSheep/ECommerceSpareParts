using Search.Application.Models.CatalogueSearch;

namespace Search.Application.Interfaces.CatalogueCandidate;

public interface ICatalogueCandidateRepository
    : ISearchRepository<Entities.CatalogueCandidate, Guid>
{
    Task<SearchResult<Entities.CatalogueCandidate>> Search(
        CatalogueSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}
