using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Attributes;
using Exceptions.Base;
using Main.Abstractions.Exceptions.Auth;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Services;
using Main.Entities;
using Main.Enums;
using Mapster;
using User = Abstractions.Models.User;
using UserInfo = Abstractions.Models.UserInfo;

namespace Main.Application.Handlers.Auth.RefreshToken;

[Transactional]
public record RefreshTokenCommand(string RefreshToken, string DeviceId) : ICommand<RefreshTokenResult>;

public record RefreshTokenResult(string Token, string RefreshToken);

public class RefreshTokenHandler(
    IUserTokenRepository tokenRepository,
    IUnitOfWork unitOfWork,
    IJwtGenerator tokenGenerator,
    IUserTokenService userTokenService,
    ITokenHasher tokenHasher,
    IUserService userService) : ICommandHandler<RefreshTokenCommand, RefreshTokenResult>
{
    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hashOfToken = tokenHasher.HashToken(request.RefreshToken);
        var queryOptions = new QueryOptions<UserToken, string>()
        {
            Data = hashOfToken,
        };
        var userToken = await tokenRepository.GetTokenByHashAsync(queryOptions, cancellationToken)
                        ?? throw new InvalidTokenException(request.RefreshToken);
        if (userToken.ExpiresAt < DateTime.UtcNow || userToken.DeviceId != request.DeviceId)
            throw new InvalidTokenException(request.RefreshToken);
        
        var user = await userService.TryGetUserAsync(userToken.UserId, cancellationToken)
            ?? throw new UserNotFoundException(userToken.UserId);

        if (user.UserInfo == null)
            throw new InternalServerException("User exists, but unable to get user info.");
        
        var (roles, permissions) = await userService
            .GetUserRolesAndPermissionsAsync(userToken.UserId, cancellationToken);

        var token = tokenGenerator.CreateToken(user.Adapt<User>(), user.UserInfo.Adapt<UserInfo>(),
            request.DeviceId, roles, permissions);
        var refreshToken = tokenGenerator.CreateRefreshToken();

        await userTokenService.AddToken(refreshToken, user.Id, TokenType.RefreshToken, DateTime.UtcNow.AddMonths(1),
            userToken.IpAddress, userToken.UserAgent, request.DeviceId, [], cancellationToken);

        unitOfWork.Remove(userToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new RefreshTokenResult(token, refreshToken);
    }
}