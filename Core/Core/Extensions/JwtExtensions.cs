using System.IdentityModel.Tokens.Jwt;
using Core.Exceptions.JwtExceptions;

namespace Core.Extensions;

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
}