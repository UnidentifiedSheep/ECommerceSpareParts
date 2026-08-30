using Enums;
using Security.Authorization;

namespace GraphQL.Common.Attributes;

public sealed class RequireAllPermissionsAttribute : RequireAuthorizationAttribute
{

	public const string PolicyName = "GraphQL.Permission.All";

	public RequireAllPermissionsAttribute(params string[] permissions) : base(
		PolicyName,
		new PermissionRequirement(permissions, AuthorizationMatch.All))
	{
	}

	public RequireAllPermissionsAttribute(params PermissionCodes[] permissions) : base(
		PolicyName,
		new PermissionRequirement(permissions, AuthorizationMatch.All))
	{
	}
}
