using Abstractions.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Exceptions.Base;
using Main.Application.Dtos.Users;
using Main.Application.Dtos.Users;
using Main.Application.Extensions.QueryExtensions;
using Main.Entities.User;
using Main.Application.Interfaces.Services;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Main.Entities.User;
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
	IReadRepository<User, Guid> userRepository,
	IProjectionProvider<User, UserDto> userProjection) : ICommandHandler<RefreshTokenCommand, RefreshTokenResult>
{
	public async Task<RefreshTokenResult> Handle(
		RefreshTokenCommand request,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.RefreshToken))
			throw new InvalidTokenException(request.RefreshToken);

		var tokenHash = valueHasher.Hash(request.RefreshToken);
		var userToken = await repository.FirstOrDefaultAsync(
				Criteria<UserToken>.New()
					.Where(x => x.TokenHash == tokenHash)
					.Track()
					.Build(),
				cancellationToken) ??
			throw new InvalidTokenException(request.RefreshToken);

		if (!userToken.IsActive || userToken.Type != TokenType.RefreshToken)
			throw new InvalidTokenException(request.RefreshToken);

		var authorization = await userRepository.GetUserAuthorizationAsync(
			userToken.UserId,
			userProjection,
			cancellationToken) ?? throw new UserNotFoundException(userToken.UserId);

		if (authorization.User.UserInfo == null)
			throw new InternalServerException("User exists, but unable to get user info.");

		var refreshToken = tokenGenerator.CreateRefreshToken();
		userToken.RotateToken(valueHasher.Hash(refreshToken));

		var token = tokenGenerator.CreateToken(
			authorization.User,
			authorization.Roles,
			authorization.Permissions);

		return new RefreshTokenResult(token, refreshToken);
	}
}
