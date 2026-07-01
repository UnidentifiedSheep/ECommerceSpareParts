using Abstractions.Interfaces;

namespace Api.Common.Extensions;

public static class UserContextExtensions
{
    private static string NormalizeRole(string role)
    {
        return role.Trim().ToUpperInvariant();
    }
    
    public static bool ContainsRole(this IUserContext userContext, string role)
    {
        return userContext.Roles.Contains(NormalizeRole(role));
    }

    public static bool ContainsRole(this IUserContext userContext, Enum role)
    {
        return userContext.Roles.Contains(NormalizeRole(role.ToString()));
    }

    public static bool ContainsPermission(this IUserContext userContext, string permission)
    {
        return userContext.Permissions.Contains(permission);
    }

    public static bool ContainsPermission(this IUserContext userContext, Enum permission)
    {
        return userContext.Permissions.Contains(permission.ToString().ToUpperInvariant().Replace('_', '.'));
    }
}
