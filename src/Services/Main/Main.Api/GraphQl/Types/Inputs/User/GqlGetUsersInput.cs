using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.User;

[GraphQLName("GetUsersInput")]
public sealed record GqlGetUsersInput
{
	[GraphQLName("pagination")]
	public required GqlPagination Pagination { get; init; }

	[GraphQLName("id")]
	public Guid? Id { get; init; }

	[GraphQLName("name")]
	public string? Name { get; init; }

	[GraphQLName("surname")]
	public string? Surname { get; init; }

	[GraphQLName("email")]
	public string? Email { get; init; }

	[GraphQLName("phone")]
	public string? Phone { get; init; }

	[GraphQLName("userName")]
	public string? UserName { get; init; }

	[GraphQLName("description")]
	public string? Description { get; init; }

	[GraphQLName("roles")]
	public IReadOnlyCollection<string>? Roles { get; init; }
}
