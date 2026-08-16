using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Search.Abstractions.Options;
using Search.Application.Interfaces.CatalogueCandidate;
using Search.Application.Models.CatalogueSearch;
using Search.Entities;
using Search.Persistence.Abstractions;
using Search.Persistence.Extensions;
using Search.Persistence.Interfaces;
using Search.Persistence.Queries;

namespace Search.Persistence;

public sealed class CatalogueCandidateRepository(
    IOptionsMonitor<OpenSearchOptions> options,
    IOpenSearchClient client,
    IIndexInitializer<CatalogueCandidate> indexInitializer)
    : OpenSearchRepository<CatalogueCandidate, Guid>(
        client,
        indexInitializer,
        () => options.CurrentValue.IndexOptions.CatalogueCandidates,
        candidate => candidate.Id),
    ICatalogueCandidateRepository
{
    public async Task<SearchResult<CatalogueCandidate>> Search(
        CatalogueSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var index = await GetIndex(cancellationToken);
        var response = await Client.SearchAsync<CatalogueCandidate>(
            search => search
                .Index(index)
                .From(criteria.Pagination.Page * criteria.Pagination.Size)
                .Size(criteria.Pagination.Size)
                .TrackTotalHits()
                .SortBySearchRelevance(criteria.SortBy, candidate => candidate.Id)
                .Query(query => CatalogueSearchQueryBuilder.Build(
                    query,
                    criteria,
                    new Field("normalizedSku"),
                    new Field("names"),
                    new Field("producerId"))),
            cancellationToken);

        EnsureResponseSucceeded(response, "search in");
        return new SearchResult<CatalogueCandidate>(
            response.Documents,
            response.Total);
    }
}
