using System.Security.Claims;

namespace Security.Extensions;

public static class JwtExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var foundValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return foundValue == null ? null : Guid.Parse(foundValue);
    }

    public static bool HasPermissions(this ClaimsPrincipal user, params string[] requiredPermissions)
    {
        var userPerms = user.FindAll("permission")
            .Select(x => x.Value)
            .ToHashSet();

        return requiredPermissions.Any(required => userPerms.Contains(required));
    }
}