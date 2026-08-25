using GraphQL.Common.Authorization;
using Security.Authorization;

namespace GraphQL.Common.Attributes;

public sealed class RequireAnyPermissionAttribute(
    params string[] permissions) : 
    RequireAuthorizationAttribute(
        PolicyName, 
        new PermissionRequirement(permissions, AuthorizationMatch.Any))
{
    public const string PolicyName = "GraphQL.Permission.Any";
}
