using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Core.Interfaces.Validators;
using Exceptions.Exceptions.Auth;
using Main.Application.Interfaces;

namespace Main.Application.Handlers.Auth.RefreshToken;

public record RefreshTokenCommand(string RefreshToken, string DeviceId) : ICommand<RefreshTokenResult>;

public record RefreshTokenResult(string Token, string RefreshToken);

public class RefreshTokenHandler(IUserTokenRepository tokenRepository, IUserRoleRepository userRoleRepository,
    IUnitOfWork unitOfWork, IJwtGenerator tokenGenerator, IUserTokenService userTokenService, 
    IUserRepository userRepository, ITokenHasher tokenHasher) : ICommandHandler<RefreshTokenCommand, RefreshTokenResult>
{
    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hashOfToken = tokenHasher.HashToken(request.RefreshToken);
        var userToken = await tokenRepository.GetTokenByHashAsync(hashOfToken, true, cancellationToken)
            ?? throw new InvalidTokenException(request.RefreshToken);
        if (userToken.ExpiresAt < DateTime.UtcNow || userToken.DeviceId != request.DeviceId)
            throw new InvalidTokenException(request.RefreshToken);
        
        User user = (await userRepository.GetUserByIdAsync(userToken.UserId, false, cancellationToken))!;
        var userRoles = (await userRoleRepository.GetUserRolesAsync(userToken.UserId, false,
            cancellationToken: cancellationToken)).Select(x => x.Role.Name).ToList();
        
        var token = tokenGenerator.CreateToken(user, user.UserInfo!, request.DeviceId, userRoles);
        var refreshToken = tokenGenerator.CreateRefreshToken();
        
        await userTokenService.AddToken(refreshToken, user.Id, TokenType.RefreshToken, DateTime.UtcNow.AddMonths(1), 
            userToken.IpAddress, userToken.UserAgent, request.DeviceId, [], cancellationToken);
        
        unitOfWork.Remove(userToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new RefreshTokenResult(token, refreshToken);
    }
}