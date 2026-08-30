using Enums;
using Security.Authorization;

namespace GraphQL.Common.Attributes;

public sealed class RequireAllRolesAttribute : RequireAuthorizationAttribute
{

	public const string PolicyName = "GraphQL.Role.All";

	public RequireAllRolesAttribute(params string[] roles) : base(
		PolicyName,
		new RoleRequirement(roles, AuthorizationMatch.All))
	{
	}

	public RequireAllRolesAttribute(params PermissionCodes[] roles) : base(
		PolicyName,
		new RoleRequirement(roles, AuthorizationMatch.All))
	{
	}
}
