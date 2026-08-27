using HotChocolate;
using HotChocolate.Types.Composite;

namespace Search.Api.GraphQl.Types;

[GraphQLName("CatalogueCandidate")]
public record GqlCatalogueCandidate(
    [property: GraphQLName("id")]
    [property: Shareable]
    Guid Id);