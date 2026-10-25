using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.User;

[GraphQLName("UserPermissionsInput")]
public sealed record GqlUserPermissionsInput
{
	[GraphQLName("userId")]
	public required Guid UserId { get; init; }

	[GraphQLName("permissions")]
	public required IReadOnlyCollection<string> Permissions { get; init; }
}
