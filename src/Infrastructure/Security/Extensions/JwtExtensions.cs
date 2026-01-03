using System.Security.Claims;

namespace Security.Extensions;

public static class JwtExtensions
{
    /// <summary>
    /// If user is not found, return false, else true.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static bool GetUserId(this ClaimsPrincipal user, out Guid userId)
    {
        userId = Guid.Empty;
        var foundValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (foundValue != null)
            userId = Guid.Parse(foundValue);
        return foundValue == null;
    }

    public static bool HasPermissions(this ClaimsPrincipal user, params string[] requiredPermissions)
    {
        var userPerms = user.FindAll("permission")
            .Select(x => x.Value)
            .ToHashSet();

        return requiredPermissions.Any(required => userPerms.Contains(required));
    }
}