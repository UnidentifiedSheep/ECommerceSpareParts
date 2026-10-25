using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.User;

[GraphQLName("SearchUsersInput")]
public sealed record GqlSearchUsersInput
{
	[GraphQLName("searchTerm")]
	public required string SearchTerm { get; init; }

	[GraphQLName("roles")]
	public IReadOnlyCollection<string>? Roles { get; init; }

	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }
}
