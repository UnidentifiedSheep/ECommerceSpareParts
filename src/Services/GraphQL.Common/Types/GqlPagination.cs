using Abstractions.Models;

namespace GraphQL.Common.Types;

[GraphQLName("Pagination")]
public record GqlPagination
{
    [GraphQLName("page")]
    public required int Page { get; init; }
    
    [GraphQLName("size")]
    public required int Size { get; init; }
    
    public static implicit operator Pagination(GqlPagination pagination)
        => new(pagination.Page, pagination.Size);
}