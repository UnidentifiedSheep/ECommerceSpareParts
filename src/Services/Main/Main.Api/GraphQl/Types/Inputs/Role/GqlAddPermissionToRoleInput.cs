using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Role;

[GraphQLName("AddPermissionToRoleInput")]
public sealed record GqlAddPermissionToRoleInput
{
	[GraphQLName("roleName")]
	public required string RoleName { get; init; }

	[GraphQLName("permissionName")]
	public required string PermissionName { get; init; }
}
