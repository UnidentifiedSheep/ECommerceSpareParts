using GraphQL.Common.Authorization;
using Security.Authorization;

namespace GraphQL.Common.Attributes;

public sealed class RequireAnyRoleAttribute(
    params string[] roles) : 
    RequireAuthorizationAttribute(
        PolicyName, 
        new RoleRequirement(roles, AuthorizationMatch.Any))
{
    public const string PolicyName = "GraphQL.Role.Any";
}
