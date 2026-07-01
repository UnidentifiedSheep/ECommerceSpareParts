using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Main.Application.Models;

public record JwtOptions
{
    public const string SectionName = "JwtBearer";

    [Required]
    public required TimeSpan ValidDuration { get; set; }

    [Required]
    public required string ValidIssuer { get; set; }

    [Required]
    public required string IssuerSigningKey { get; set; }

    public SigningCredentials SigningCredentials
    {
        get
        {
            if (field != null) return field;
            var key = Encoding.UTF8.GetBytes(IssuerSigningKey);
            field = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256);
            return field;
        }
    } = null;
}