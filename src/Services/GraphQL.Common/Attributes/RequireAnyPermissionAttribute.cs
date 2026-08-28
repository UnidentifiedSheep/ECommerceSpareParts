using Enums;
using GraphQL.Common.Authorization;
using Security.Authorization;

namespace GraphQL.Common.Attributes;

public sealed class RequireAnyPermissionAttribute : 
    RequireAuthorizationAttribute
{
    public RequireAnyPermissionAttribute(params string[] permissions) : 
        base(PolicyName, new PermissionRequirement(permissions, AuthorizationMatch.Any)) { }
    
    public RequireAnyPermissionAttribute(params PermissionCodes[] permissions) : 
        base(PolicyName, new PermissionRequirement(permissions, AuthorizationMatch.Any)) { }
    
    public const string PolicyName = "GraphQL.Permission.Any";
}
