using System.Net;
using System.Security.Cryptography;
using Application.Common.Interfaces;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Interfaces.Validators;
using Core.Models;
using Exceptions.Base;
using Exceptions.Exceptions.Auth;
using Main.Core.Enums;
using Main.Core.Interfaces.DbRepositories;
using Main.Core.Interfaces.Services;
using Mapster;

namespace Main.Application.Handlers.Auth.Login;

public record LoginCommand(string Email, string Password, IPAddress? IpAddress, string? UserAgent)
    : ICommand<LoginResult>;

public record LoginResult(string Token, string RefreshToken, string DeviceId);

public class LoginHandler(
    IPasswordManager passwordManager,
    IUserEmailRepository userEmailRepository,
    IUserRoleRepository userRoleRepository,
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

        var roles = (await userRoleRepository.GetUserRolesAsync(user.Id, false,
            cancellationToken: cancellationToken)).Select(x => x.Role.NormalizedName).ToList();

        var deviceId = GenerateDeviceId();
        var ip = request.IpAddress;
        var userAgent = request.UserAgent;

        var token = tokenGenerator.CreateToken(user.Adapt<User>(), user.UserInfo.Adapt<UserInfo>(), deviceId, roles);
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