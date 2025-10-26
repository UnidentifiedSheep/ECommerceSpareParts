using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Core.Interfaces;
using Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Security;

public class JwtGenerator(IConfiguration configuration) : IJwtGenerator
{
    public string CreateToken(User user, UserInfo userInfo, string deviceId, IEnumerable<string> roles)
    {
        var handler = new JwtSecurityTokenHandler();
        var privateKey = Encoding.UTF8.GetBytes(configuration["JwtBearer:IssuerSigningKey"]!);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(privateKey),
            SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            SigningCredentials = credentials,
            Issuer = configuration["JwtBearer:ValidIssuer"],
            Expires = DateTime.UtcNow.AddMinutes(5),
            Subject = GetClaims(user, userInfo, deviceId, roles)
        };
        return handler.WriteToken(handler.CreateToken(tokenDescriptor));
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
            ValidIssuer = configuration["JwtBearer:ValidIssuer"],
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtBearer:IssuerSigningKey"]!))
        };
        return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
    }

    private ClaimsIdentity GetClaims(User user, UserInfo userInfo, string deviceId, IEnumerable<string> roles)
    {
        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim(ClaimTypes.GivenName, userInfo.Name + " " + userInfo.Surname));
        claims.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
        claims.AddClaim(new Claim("device_id", deviceId));
        claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        foreach (var role in roles)
            claims.AddClaim(new Claim(ClaimTypes.Role, role));

        return claims;
    }
}