using Security.Authorization;

namespace GraphQL.Common.Attributes;

public sealed class RequireAllRolesAttribute(
    params string[] roles) :
    RequireAuthorizationAttribute(
        PolicyName,
        new RoleRequirement(roles, AuthorizationMatch.All))
{
    public const string PolicyName = "GraphQL.Role.All";
}
