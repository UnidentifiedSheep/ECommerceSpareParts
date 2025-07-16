using System.Security.Claims;

namespace MonoliteUnicorn.Services.JWT;

public static class JwtHelper
{
    public static string? GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier);


    public static List<string> GetUserRoles(this ClaimsPrincipal user)
        => user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
}