using HotChocolate;

namespace Main.Api.GraphQl.Queries.Root;

public sealed class RootQuery
{
    [GraphQLName("products")]
    public readonly ProductQueries ProductQueries = new();
}