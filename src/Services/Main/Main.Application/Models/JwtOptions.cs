using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Main.Application.Models;

public record JwtOptions
{
    public JwtOptions(string issuerSignKey, string validIssuer, TimeSpan validDuration)
    {
        var key = Encoding.UTF8.GetBytes(issuerSignKey);
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256);
        ValidIssuer = validIssuer;
        ValidDuration = validDuration;
    }
    public TimeSpan ValidDuration { get; }
    public SigningCredentials SigningCredentials { get; init; }
    public string ValidIssuer { get; }
}