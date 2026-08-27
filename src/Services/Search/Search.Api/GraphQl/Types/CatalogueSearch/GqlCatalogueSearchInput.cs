using GraphQL.Common.Types;
using HotChocolate;
using Search.Enums;

namespace Search.Api.GraphQl.Types.CatalogueSearch;

[GraphQLName("CatalogueSearchInput")]
public record GqlCatalogueSearchInput
{
    [GraphQLName("query")]
    public string? Query { get; init; }

    [GraphQLName("targets")]
    public required IReadOnlyCollection<SearchTarget> Targets { get; init; }

    [GraphQLName("skuModes")]
    public required IReadOnlyCollection<SearchMatchType> SkuModes { get; init; }

    [GraphQLName("nameModes")]
    public required IReadOnlyCollection<SearchMatchType> NameModes { get; init; }

    [GraphQLName("producerIds")]
    public IReadOnlyCollection<int>? ProducerIds { get; init; } = [];

    [GraphQLName("pagination")]
    public required GqlPagination Pagination { get; init; }

    [GraphQLName("productSortBy")]
    public IReadOnlyCollection<GqlSortBy>? ProductSortBy { get; init; }

    [GraphQLName("catalogueCandidateSortBy")]
    public IReadOnlyCollection<GqlSortBy>? CatalogueCandidateSortBy { get; init; }

    [GraphQLName("includeHighlights")]
    public bool? IncludeHighlights { get; init; }
}
