using System.Net;
using Abstractions.Interfaces;
using Application.Common.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities.Auth;
using Main.Enums;
using Security.Core.Interfaces;

namespace Main.Application.Services;

public class UserTokenService(IUnitOfWork unitOfWork, IValueHasher valueHasher) : IUserTokenService
{
	public async Task AddToken(
		string token,
		Guid userId,
		TokenType type,
		DateTime exp,
		IPAddress? ip,
		string? userAgent,
		string? deviceId,
		IEnumerable<string> permissions,
		CancellationToken cancellationToken = default)
	{
		var tokenModel = new UserToken
		{
			TokenHash = valueHasher.Hash(token),
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
