using Abstractions;

namespace GraphQL.Common.Types;

[GraphQLName("SortBy")]
public record GqlSortBy
{
    [GraphQLName("field")]
    public required string Field { get; init; }
    
    [GraphQLName("isDescending")]
    public required bool IsDescending { get; init; }

    public override string ToString()
    {
        var dir = IsDescending ? "desc" : "asc";
        return $"{Field}{QueryableSortBy.Value.Delimiter}{dir}";
    }
}