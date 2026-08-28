using HotChocolate;

namespace Search.Api.GraphQl.Types.CatalogueSearch;

[GraphQLName("CatalogueSearchResult")]
public record GqlCatalogueSearchResult
{
    [GraphQLName("products")]
    public required GqlSearchCatalogueSection<GqlProduct> Products { get; init; }
    
    [GraphQLName("catalogues")]
    public required GqlSearchCatalogueSection<GqlCatalogueCandidate> Candidates { get; init; }
}