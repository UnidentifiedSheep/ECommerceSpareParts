using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Exceptions.Base;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Interfaces.Services;
using Main.Entities.Auth;
using Main.Entities.Exceptions.Auth;
using Main.Enums;

namespace Main.Application.Handlers.Auth.RefreshToken;

[Transactional]
public record RefreshTokenCommand(string RefreshToken, string DeviceId) : ICommand<RefreshTokenResult>;

public record RefreshTokenResult(string Token, string RefreshToken);

public class RefreshTokenHandler(
    IRepository<UserToken, Guid> repository,
    IUnitOfWork unitOfWork,
    IJwtGenerator tokenGenerator,
    IUserTokenService userTokenService,
    ITokenHasher tokenHasher,
    IUserService userService) : ICommandHandler<RefreshTokenCommand, RefreshTokenResult>
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
        
        var user = await userService.TryGetUserAsync(userToken.UserId, cancellationToken)
            ?? throw new UserNotFoundException(userToken.UserId);

        if (user.UserInfo == null)
            throw new InternalServerException("User exists, but unable to get user info.");
        
        var (roles, permissions) = await userService
            .GetUserRolesAndPermissionsAsync(userToken.UserId, cancellationToken);
        
        var token = tokenGenerator.CreateToken(user, request.DeviceId, roles, permissions);
        var refreshToken = tokenGenerator.CreateRefreshToken();

        await userTokenService.AddToken(refreshToken, user.Id, TokenType.RefreshToken, DateTime.UtcNow.AddMonths(1),
            userToken.IpAddress, userToken.UserAgent, request.DeviceId, [], cancellationToken);

        unitOfWork.Remove(userToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new RefreshTokenResult(token, refreshToken);
    }
}