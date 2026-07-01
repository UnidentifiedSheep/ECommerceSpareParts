using System.Text;

namespace Integrations.Common;

public record BasicAuthModel
{
    public required string Login { get; init; }
    public required string Password { get; init; }

    public string GetToken()
        => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Login}:{Password}"));
}