using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.User;

[GraphQLName("UserRolesInput")]
public sealed record GqlUserRolesInput
{
	[GraphQLName("userId")]
	public required Guid UserId { get; init; }

	[GraphQLName("roles")]
	public required IReadOnlyCollection<string> Roles { get; init; }
}
