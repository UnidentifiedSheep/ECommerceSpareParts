using HotChocolate;

namespace Search.Api.GraphQl.Types;

[GraphQLName("CatalogueCandidate")]
public record GqlCatalogueCandidate(
    [property: GraphQLName("id")]
    Guid Id);