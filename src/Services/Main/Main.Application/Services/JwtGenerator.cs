using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Main.Application.Dtos.Users;
using Main.Application.Interfaces.Services;
using Main.Application.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Main.Application.Services;

public class JwtGenerator(IOptions<JwtOptions> options) : IJwtGenerator
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public string CreateToken(
        UserDto user,
        string deviceId,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        TimeSpan? additionalValidDuration = null)
    {
        var validDuration = options.Value.ValidDuration + (additionalValidDuration ?? TimeSpan.Zero);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            SigningCredentials = options.Value.SigningCredentials,
            Issuer = options.Value.ValidIssuer,
            Expires = DateTime.UtcNow.Add(validDuration),
            Subject = GetClaims(
                user,
                deviceId,
                roles,
                permissions)
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
            ValidIssuer = options.Value.ValidIssuer,
            IssuerSigningKey = options.Value.SigningCredentials.Key
        };
        return new JwtSecurityTokenHandler().ValidateToken(
            token,
            validation,
            out _);
    }

    private ClaimsIdentity GetClaims(
        UserDto user,
        string deviceId,
        IEnumerable<string> roles,
        IEnumerable<string> permissions)
    {
        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim(ClaimTypes.GivenName, user.UserInfo?.Name + " " + user.UserInfo?.Surname));
        claims.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
        claims.AddClaim(new Claim("device_id", deviceId));
        claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        foreach (var role in roles) claims.AddClaim(new Claim(ClaimTypes.Role, role));
        foreach (var permission in permissions) claims.AddClaim(new Claim("permission", permission));

        return claims;
    }
}