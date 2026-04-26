using System.Net;
using System.Security.Cryptography;
using Abstractions.Interfaces.Validators;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Exceptions.Base;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Projections;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities.Exceptions.Auth;
using Main.Enums;

namespace Main.Application.Handlers.Auth.Login;

[AutoSave]
[Transactional]
public record LoginCommand(string Email, string Password, IPAddress? IpAddress, string? UserAgent)
    : ICommand<LoginResult>;

public record LoginResult(string Token, string RefreshToken, string DeviceId);

public class LoginHandler(
    IPasswordManager passwordManager,
    IUserRepository userRepository,
    IUserTokenService userTokenService,
    IJwtGenerator tokenGenerator,
    IUserService userService) : ICommandHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var criteria = Criteria<Entities.User.User>.New()
            .Include(x => x.UserInfo)
            .Build();
        
        var user = await userRepository.GetUserByPrimaryEmailAsync(request.Email, criteria, cancellationToken)
            ?? throw new WrongCredentialsException(request.Email, null);
        
        if (user.UserInfo == null)
            throw new InternalServerException("User exists, but unable to get user info.");
        if (!passwordManager.VerifyHashedPassword(user.PasswordHash, request.Password))
            throw new WrongCredentialsException(request.Email, request.Password);

        var (roles, permissions) =
            await userService.GetUserRolesAndPermissionsAsync(user.Id, cancellationToken);

        var deviceId = GenerateDeviceId();
        var ip = request.IpAddress;
        var userAgent = request.UserAgent;

        UserDto userDto = UserProjections.UserProjectionFunc(user);
        var token = tokenGenerator.CreateToken(userDto, deviceId, roles, permissions);
        var refreshToken = tokenGenerator.CreateRefreshToken();

        await userTokenService.AddToken(refreshToken, user.Id, TokenType.RefreshToken, DateTime.UtcNow.AddMonths(1),
            ip, userAgent, deviceId, [], cancellationToken);

        user.Login();

        return new LoginResult(token, refreshToken, deviceId);
    }

    private string GenerateDeviceId()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes);
    }
}