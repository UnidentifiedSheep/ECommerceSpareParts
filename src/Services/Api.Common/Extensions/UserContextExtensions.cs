using Abstractions.Interfaces;
using Extensions;

namespace Api.Common.Extensions;

public static class UserContextExtensions
{
    public static bool ContainsRole(this IUserContext userContext, string role)
    {
        return userContext.Roles.Contains(role);
    }
    
    public static bool ContainsPermission(this IUserContext userContext, string permission)
    {
        return userContext.Permissions.Contains(permission);
    }
    
    public static bool ContainsPermission(this IUserContext userContext, Enum permission)
    {
        return userContext.Permissions.Contains(permission.ToNormalizedPermission());
    }
}