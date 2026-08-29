using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("ProductCrossesInput")]
public sealed record GqlProductCrossesInput
{
    [GraphQLName("pagination")]
    public required GqlPagination Pagination { get; init; }

    [GraphQLName("sortBy")]
    public IReadOnlyCollection<GqlSortBy>? SortBy { get; init; }
}
