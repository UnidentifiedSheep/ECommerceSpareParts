using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Abstractions.Interfaces;
using Abstractions.Models;
using Microsoft.IdentityModel.Tokens;
using Security.Models;

namespace Security.Services;

public class JwtGenerator(JwtOptions options) : IJwtGenerator
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    public string CreateToken(
        User user,
        UserInfo userInfo,
        string deviceId,
        IEnumerable<string> roles,
        IEnumerable<string> permissions)
    {

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            SigningCredentials = options.SigningCredentials,
            Issuer = options.ValidIssuer,
            Expires = DateTime.UtcNow.Add(options.ValidDuration),
            Subject = GetClaims(user, userInfo, deviceId, roles, permissions)
        };
        return _tokenHandler.WriteToken(_tokenHandler.CreateToken(tokenDescriptor));
    }

    public string CreateRefreshToken()
    {
        var randomNum = new byte[64];
        using var numGenerator = RandomNumberGenerator.Create();
        numGenerator.GetBytes(randomNum);

        return Convert.ToBase64String(randomNum);
    }

    public ClaimsPrincipal GetClaimsPrincipal(string token)
    {
        var validation = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = options.ValidIssuer,
            IssuerSigningKey = options.SigningCredentials.Key
        };
        return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
    }

    private ClaimsIdentity GetClaims(
        User user,
        UserInfo userInfo,
        string deviceId,
        IEnumerable<string> roles,
        IEnumerable<string> permissions)
    {
        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim(ClaimTypes.GivenName, userInfo.Name + " " + userInfo.Surname));
        claims.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
        claims.AddClaim(new Claim("device_id", deviceId));
        claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        foreach (var role in roles)
            claims.AddClaim(new Claim(ClaimTypes.Role, role));
        foreach (var permission in permissions)
            claims.AddClaim(new Claim("permission", permission));

        return claims;
    }
}