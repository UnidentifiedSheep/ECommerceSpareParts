using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Exceptions.Exceptions.JwtExceptions;

namespace Security.Extensions;

public static class JwtExtensions
{
    public static string? ExtractUserId(this string token)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token)) throw new InvalidTokenException(token);

        var jwtToken = handler.ReadJwtToken(token);
        var nameId = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "nameid")?.Value;
        return nameId;
    }
    
    public static string? GetUserId(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.NameIdentifier)?.Value;


    public static List<string> GetUserRoles(this ClaimsPrincipal user)
        => user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
}