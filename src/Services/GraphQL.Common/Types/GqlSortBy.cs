using Application.Common.Querying;

namespace GraphQL.Common.Types;

[GraphQLName("SortBy")]
public record GqlSortBy
{
	[GraphQLName("field")]
	public required string Field { get; init; }

	[GraphQLName("isDescending")]
	public required bool IsDescending { get; init; }

	[GraphQLName("argument")]
	public string? Argument { get; init; }

	[GraphQLIgnore]
	public SortExpression ToSortExpression() => new(Field, IsDescending, Argument);

	public static implicit operator SortExpression?(GqlSortBy? sortBy) => sortBy?.ToSortExpression();
}
