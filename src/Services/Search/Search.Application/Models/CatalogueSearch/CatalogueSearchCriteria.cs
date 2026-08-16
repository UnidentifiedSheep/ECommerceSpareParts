using Abstractions.Models;
using Search.Enums;

namespace Search.Application.Models.CatalogueSearch;

public sealed record CatalogueSearchCriteria
{
    public required string Query { get; init; }

    public required IReadOnlySet<SearchMatchType> SkuModes { get; init; }

    public required IReadOnlySet<SearchMatchType> NameModes { get; init; }

    public IReadOnlyCollection<int> ProducerIds { get; init; } = [];

    public required Pagination Pagination { get; init; }

    public string[] SortBy { get; init; } = [];

    public bool IncludeHighlights { get; init; }
}
