using System.Text;

namespace Armtek.Integrations.Core.Models;

public record BasicAuthModel
{
    public required string Login { get; init; }
    public required string Password { get; init; }

    public string GetToken()
        => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Login}:{Password}"));
}