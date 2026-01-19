using System.Net;
using Main.Enums;

namespace Main.Abstractions.Interfaces.Services;

public interface IUserTokenService
{
    Task AddToken(string token, Guid userId, TokenType type, DateTime exp, IPAddress? ip,
        string? userAgent, string? deviceId, IEnumerable<string> permissions,
        CancellationToken cancellationToken = default);
}