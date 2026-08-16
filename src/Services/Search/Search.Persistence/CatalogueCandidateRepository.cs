using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Search.Abstractions.Options;
using Search.Application.Interfaces.CatalogueCandidate;
using Search.Entities;
using Search.Persistence.Abstractions;
using Search.Persistence.Interfaces;

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
    ICatalogueCandidateRepository;
