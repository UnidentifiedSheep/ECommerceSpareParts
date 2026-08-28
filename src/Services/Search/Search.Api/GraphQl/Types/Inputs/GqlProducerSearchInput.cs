using GraphQL.Common.Types;
using HotChocolate;

namespace Search.Api.GraphQl.Types.Inputs;

[GraphQLName("ProducerSearchInput")]
public record GqlProducerSearchInput
{
    [GraphQLName("query")]
    public string? Query { get; init; }
    
    [GraphQLName("pagination")]
    public required GqlPagination Pagination { get; init; }
}