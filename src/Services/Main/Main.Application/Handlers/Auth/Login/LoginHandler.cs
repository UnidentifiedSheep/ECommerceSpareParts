using System.Net;
using System.Security.Cryptography;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Abstractions.Interfaces.Validators;
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

namespace Main.Application.Handlers.Auth.Login;

[Transactional]
public record LoginCommand(string Email, string Password, IPAddress? IpAddress, string? UserAgent)
    : ICommand<LoginResult>;

public record LoginResult(string Token, string RefreshToken, string DeviceId);

public class LoginHandler(
    IPasswordManager passwordManager,
    IUserEmailRepository userEmailRepository,
    IUserTokenService userTokenService,
    IJwtGenerator tokenGenerator,
    IUserService userService,
    IUnitOfWork unitOfWork) : ICommandHandler<LoginCommand, LoginResult>
{
    private static readonly QueryOptions<UserEmail> UserQueryOptions =
        new QueryOptions<UserEmail>()
            .WithTracking()
            .WithInclude(x => x.User)
            .WithInclude(x => x.User.UserInfo);
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userEmail = await userEmailRepository.GetPrimaryUserEmail(request.Email, UserQueryOptions, cancellationToken)
                   ?? throw new WrongCredentialsException(request.Email, null);
        var user = userEmail.User;
        if (user.UserInfo == null)
            throw new InternalServerException("User exists, but unable to get user info.");
        if (!passwordManager.VerifyHashedPassword(user.PasswordHash, request.Password))
            throw new WrongCredentialsException(request.Email, request.Password);

        var (roles, permissions) = 
            await userService.TryGetUserRolesAndPermissionsAsync(user.Id, cancellationToken);

        var deviceId = GenerateDeviceId();
        var ip = request.IpAddress;
        var userAgent = request.UserAgent;

        var token = tokenGenerator.CreateToken(user.Adapt<User>(), user.UserInfo.Adapt<UserInfo>(), deviceId, roles,
            permissions);
        var refreshToken = tokenGenerator.CreateRefreshToken();

        await userTokenService.AddToken(refreshToken, user.Id, TokenType.RefreshToken, DateTime.UtcNow.AddMonths(1),
            ip, userAgent, deviceId, [], cancellationToken);

        user.LastLoginAt = DateTime.UtcNow;
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResult(token, refreshToken, deviceId);
    }

    private string GenerateDeviceId()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes);
    }
}