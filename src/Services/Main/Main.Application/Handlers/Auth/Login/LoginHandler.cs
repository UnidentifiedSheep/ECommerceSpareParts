using System.Net;
using System.Security.Cryptography;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Abstractions.Interfaces.Validators;
using Abstractions.Models;
using Application.Common.Interfaces;
using Attributes;
using Exceptions.Base;
using Exceptions.Exceptions.Auth;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Services;
using Main.Enums;
using Mapster;

namespace Main.Application.Handlers.Auth.Login;

[Transactional]
public record LoginCommand(string Email, string Password, IPAddress? IpAddress, string? UserAgent)
    : ICommand<LoginResult>;

public record LoginResult(string Token, string RefreshToken, string DeviceId);

public class LoginHandler(
    IPasswordManager passwordManager,
    IUserEmailRepository userEmailRepository,
    IRolePermissionService rolePermissionService,
    IUserTokenService userTokenService,
    IJwtGenerator tokenGenerator,
    IUnitOfWork unitOfWork) : ICommandHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userEmailRepository.GetUserByPrimaryMailAsync(request.Email, false, cancellationToken)
                   ?? throw new WrongCredentialsException(request.Email);
        if (user.UserInfo == null)
            throw new InternalServerException("Что то пошло не так...");
        if (!passwordManager.VerifyHashedPassword(user.PasswordHash, request.Password))
            throw new WrongCredentialsException(request.Email + request.Password);

        var (roles, permissions) = await rolePermissionService
            .GetUserPermissionsAsync(user.Id, cancellationToken);

        var deviceId = GenerateDeviceId();
        var ip = request.IpAddress;
        var userAgent = request.UserAgent;

        var token = tokenGenerator.CreateToken(user.Adapt<User>(), user.UserInfo.Adapt<UserInfo>(), deviceId, roles, permissions);
        var refreshToken = tokenGenerator.CreateRefreshToken();

        await userTokenService.AddToken(refreshToken, user.Id, TokenType.RefreshToken, DateTime.UtcNow.AddMonths(1),
            ip, userAgent, deviceId, [], cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResult(token, refreshToken, deviceId);
    }

    private string GenerateDeviceId()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes);
    }
}