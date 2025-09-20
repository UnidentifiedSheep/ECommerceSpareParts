using System.Net;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Services;
using Core.Interfaces.Validators;

namespace Application.Services;

public class UserTokenService(IUnitOfWork unitOfWork, IPasswordManager passwordManager) : IUserTokenService
{
    public async Task AddToken(string token, Guid userId, TokenType type, DateTime exp, IPAddress? ip,
        string? userAgent, string? deviceId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        var tokenModel = new UserToken
        {
            TokenHash = passwordManager.GetHashOfPassword(token),
            UserId = userId,
            Type = type.ToString(),
            Permissions = permissions.ToList(),
            ExpiresAt = exp,
            IpAddress = ip,
            UserAgent = userAgent,
            DeviceId = deviceId
        };
        await unitOfWork.AddAsync(tokenModel, cancellationToken);
    }
}