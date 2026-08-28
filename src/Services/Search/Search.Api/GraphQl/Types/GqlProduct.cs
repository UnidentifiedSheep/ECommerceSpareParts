using HotChocolate;
using HotChocolate.Types.Composite;

namespace Search.Api.GraphQl.Types;

[GraphQLName("Product")]
public record GqlProduct(
    [property: GraphQLName("id")]
    [property: Shareable]
    int Id);