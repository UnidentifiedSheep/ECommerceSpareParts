using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Role;

[GraphQLName("GetRolesInput")]
public sealed record GqlGetRolesInput
{
	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("searchTerm")]
	public string? SearchTerm { get; init; }
}
