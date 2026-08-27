using Enums;
using Security.Authorization;

namespace GraphQL.Common.Attributes;

public sealed class RequireAllRolesAttribute : RequireAuthorizationAttribute
{
    public RequireAllRolesAttribute(
        params string[] roles) : 
        base(
            PolicyName, 
            new RoleRequirement(roles, AuthorizationMatch.All)) { }
    
    public RequireAllRolesAttribute(
        params PermissionCodes[] roles) : 
        base(
            PolicyName, 
            new RoleRequirement(roles, AuthorizationMatch.All)) { }
    
    public const string PolicyName = "GraphQL.Role.All";
}
