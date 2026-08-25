using GraphQL.Common.Authorization;
using Security.Authorization;

namespace GraphQL.Common.Attributes;

public sealed class RequireAllPermissionsAttribute(
    params string[] permissions) : 
    RequireAuthorizationAttribute(
        PolicyName, 
        new PermissionRequirement(permissions, AuthorizationMatch.All))
{
    public const string PolicyName = "GraphQL.Permission.All";
}
