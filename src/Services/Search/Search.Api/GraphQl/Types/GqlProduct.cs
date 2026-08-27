using HotChocolate;

namespace Search.Api.GraphQl.Types;

[GraphQLName("Product")]
public record GqlProduct(
    [property: GraphQLName("id")]
    int Id);