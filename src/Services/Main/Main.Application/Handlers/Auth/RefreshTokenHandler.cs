using Abstractions.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Exceptions.Base;
using Main.Application.Interfaces.Cache;
using Main.Application.Interfaces.Services;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Main.Enums;
using Security.Core.Interfaces;

namespace Main.Application.Handlers.Auth;

[Transactional(retryDelayMs: 10, retryCount: 2)]
[AutoSave]
public record RefreshTokenCommand(string RefreshToken) : ICommand<RefreshTokenResult>;

public record RefreshTokenResult(string Token, string RefreshToken);

public class RefreshTokenHandler(
	IRepository<UserToken, Guid> repository,
	IJwtGenerator tokenGenerator,
	IValueHasher valueHasher,
	IUserCacheRepository userCache) : ICommandHandler<RefreshTokenCommand, RefreshTokenResult>
{
	public async Task<RefreshTokenResult> Handle(
		RefreshTokenCommand request,
		CancellationToken cancellationToken)
	{
		var userToken = await repository.FirstOrDefaultAsync(
				Criteria<UserToken>.New()
					.Where(x => x.TokenHash == valueHasher.Hash(request.RefreshToken))
					.Track()
					.Build(),
				cancellationToken) ??
			throw new InvalidTokenException(request.RefreshToken);

		if (userToken.IsActive)
			throw new InvalidTokenException(request.RefreshToken);

		var user = await userCache.TryGetUserAsync(userToken.UserId, cancellationToken) ??
			throw new UserNotFoundException(userToken.UserId);

		if (user.UserInfo == null)
			throw new InternalServerException("User exists, but unable to get user info.");

		var (roles, permissions) =
			await userCache.GetUserRolesAndPermissionsAsync(userToken.UserId, cancellationToken) ??
			throw new UserNotFoundException(userToken.UserId);

		var refreshToken = tokenGenerator.CreateRefreshToken();
		userToken.RotateToken(valueHasher.Hash(refreshToken));

		var token = tokenGenerator.CreateToken(
			user,
			roles,
			permissions);

		return new RefreshTokenResult(token, refreshToken);
	}
}
