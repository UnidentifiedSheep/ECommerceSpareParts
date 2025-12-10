using System.Security.Claims;

namespace Security.Extensions;

public static class JwtExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public static bool HasPermissions(this ClaimsPrincipal user, params string[] requiredPermissions)
    {
        var userPerms = user.FindAll("permission")
            .Select(x => x.Value)
            .ToHashSet();

        return requiredPermissions.Any(required => userPerms.Contains(required));
    }
}