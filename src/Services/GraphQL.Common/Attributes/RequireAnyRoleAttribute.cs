using Enums;
using Security.Authorization;

namespace GraphQL.Common.Attributes;

public sealed class RequireAnyRoleAttribute : RequireAuthorizationAttribute
{

	public const string PolicyName = "GraphQL.Role.Any";

	public RequireAnyRoleAttribute(params string[] roles) : base(
		PolicyName,
		new RoleRequirement(roles, AuthorizationMatch.Any))
	{
	}

	public RequireAnyRoleAttribute(params PermissionCodes[] roles) : base(
		PolicyName,
		new RoleRequirement(roles, AuthorizationMatch.Any))
	{
	}
}
