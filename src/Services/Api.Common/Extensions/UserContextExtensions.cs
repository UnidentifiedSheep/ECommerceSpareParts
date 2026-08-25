using Abstractions.Interfaces;
using Security.Authorization;

namespace Api.Common.Extensions;

public static class UserContextExtensions
{
    public static bool ContainsRole(this IUserContext userContext, string role)
    {
        return userContext.Roles.Contains(AuthorizationValueNormalizer.NormalizeRole(role));
    }

    public static bool ContainsRole(this IUserContext userContext, Enum role)
    {
        return userContext.Roles.Contains(AuthorizationValueNormalizer.NormalizeRole(role.ToString()));
    }

    public static bool ContainsPermission(this IUserContext userContext, string permission)
    {
        return userContext.Permissions.Contains(AuthorizationValueNormalizer.NormalizePermission(permission));
    }

    public static bool ContainsPermission(this IUserContext userContext, Enum permission)
    {
        return userContext.Permissions.Contains(
            AuthorizationValueNormalizer.NormalizePermission(permission.ToString()));
    }
}
