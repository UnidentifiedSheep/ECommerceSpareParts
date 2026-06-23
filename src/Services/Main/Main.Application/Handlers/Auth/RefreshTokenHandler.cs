using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Exceptions.Base;
using Main.Application.Interfaces.Cache;
using Main.Application.Interfaces.Services;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Main.Enums;

namespace Main.Application.Handlers.Auth;

[Diagnostics(maxExecutionTimeMs: 300)]
[Transactional, AutoSave]
public record RefreshTokenCommand(string RefreshToken, string DeviceId) : ICommand<RefreshTokenResult>;

public record RefreshTokenResult(string Token, string RefreshToken);

public class RefreshTokenHandler(
    IRepository<UserToken, Guid> repository,
    IUnitOfWork unitOfWork,
    IJwtGenerator tokenGenerator,
    IUserTokenService userTokenService,
    ITokenHasher tokenHasher,
    IUserCacheRepository userCache) : ICommandHandler<RefreshTokenCommand, RefreshTokenResult>
{
    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hashOfToken = tokenHasher.HashToken(request.RefreshToken);
        var criteria = Criteria<UserToken>.New()
            .Where(x => x.TokenHash == hashOfToken)
            .Build();

        var userToken = await repository.FirstOrDefaultAsync(criteria, cancellationToken)
                        ?? throw new InvalidTokenException(request.RefreshToken);
        if (userToken.ExpiresAt < DateTime.UtcNow || userToken.DeviceId != request.DeviceId)
            throw new InvalidTokenException(request.RefreshToken);

        var user = await userCache.TryGetUserAsync(userToken.UserId, cancellationToken)
                   ?? throw new UserNotFoundException(userToken.UserId);

        if (user.UserInfo == null)
            throw new InternalServerException("User exists, but unable to get user info.");

        var (roles, permissions) = await userCache
                                       .GetUserRolesAndPermissionsAsync(userToken.UserId, cancellationToken)
                                   ?? throw new UserNotFoundException(userToken.UserId);

        var token = tokenGenerator.CreateToken(user, request.DeviceId, roles, permissions);
        var refreshToken = tokenGenerator.CreateRefreshToken();

        await userTokenService.AddToken(refreshToken, user.Id, TokenType.RefreshToken, DateTime.UtcNow.AddMonths(1),
            userToken.IpAddress, userToken.UserAgent, request.DeviceId, [], cancellationToken);

        unitOfWork.Remove(userToken);
        return new RefreshTokenResult(token, refreshToken);
    }
}