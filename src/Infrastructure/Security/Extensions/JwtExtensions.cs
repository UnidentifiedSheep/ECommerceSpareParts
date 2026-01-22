using System.Security.Claims;

namespace Security.Extensions;

public static class JwtExtensions
{
    /// <summary>
    /// Gets the user ID from the ClaimsPrincipal.
    /// </summary>
    /// <param name="user">Clim</param>
    /// <param name="userId"></param>
    /// <returns>Returns true if clim contains user id, else returns false</returns>
    public static bool GetUserId(this ClaimsPrincipal user, out Guid userId)
    {
        userId = Guid.Empty;
        var foundValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(foundValue)) 
            return false;
        return Guid.TryParse(foundValue, out userId);
    }

    public static bool HasPermissions(this ClaimsPrincipal user, params string[] requiredPermissions)
    {
        var userPerms = user.FindAll("permission")
            .Select(x => x.Value)
            .ToHashSet();

        return requiredPermissions.Any(required => userPerms.Contains(required));
    }
}