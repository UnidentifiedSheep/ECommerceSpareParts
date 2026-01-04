using System.Net;
using Core.Interfaces;
using Core.Interfaces.Services;
using Main.Core.Entities;
using Main.Core.Enums;
using Main.Core.Interfaces.Services;

namespace Main.Application.Services;

public class UserTokenService(IUnitOfWork unitOfWork, ITokenHasher tokenHasher) : IUserTokenService
{
    public async Task AddToken(string token, Guid userId, TokenType type, DateTime exp, IPAddress? ip,
        string? userAgent, string? deviceId, IEnumerable<string> permissions,
        CancellationToken cancellationToken = default)
    {
        var tokenModel = new UserToken
        {
            TokenHash = tokenHasher.HashToken(token),
            UserId = userId,
            Type = type,
            Permissions = permissions.ToList(),
            ExpiresAt = exp,
            IpAddress = ip,
            UserAgent = userAgent,
            DeviceId = deviceId
        };
        await unitOfWork.AddAsync(tokenModel, cancellationToken);
    }
}