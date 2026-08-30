using System.Net;
using System.Security.Cryptography;
using Abstractions.Interfaces.Validators;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Enums;
using Exceptions.Base;
using Main.Application.Dtos.Users;
using Main.Application.Extensions;
using Main.Application.Interfaces.Cache;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Main.Enums;

namespace Main.Application.Handlers.Auth;

[Transactional]
[AutoSave]
public record LoginCommand(
	string Login,
	string Password,
	IPAddress? IpAddress,
	string? UserAgent) : ICommand<LoginResult>;

public record LoginResult(string Token, string RefreshToken, string DeviceId);

public class LoginHandler(
	IEmailValidator emailValidator,
	IPasswordManager passwordManager,
	IUserRepository userRepository,
	IUserTokenService userTokenService,
	IJwtGenerator tokenGenerator,
	IUserCacheRepository userCache,
	IProjectionProvider<User, UserDto> userProjection) : ICommandHandler<LoginCommand, LoginResult>
{
	public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
	{
		var criteria = Criteria<User>
			.New()
			.WhereDoesNotHaveRole(Role.System)
			.Include(x => x.UserInfo)
			.Track()
			.Build();

		var user = await userRepository.GetUserByLoginAsync(
			request.Login,
			emailValidator.IsValidEmail(request.Login),
			criteria,
			cancellationToken) ?? throw new WrongCredentialsException(request.Login, null);

		if (user.UserInfo == null)
			throw new InternalServerException("User exists, but unable to get user info.");
		if (!passwordManager.VerifyHashedPassword(user.PasswordHash, request.Password))
			throw new WrongCredentialsException(request.Login, request.Password);

		var (roles, permissions) =
			await userCache.GetUserRolesAndPermissionsAsync(user.Id, cancellationToken) ??
			throw new UserNotFoundException(user.Id);

		var deviceId = GenerateDeviceId();
		var ip = request.IpAddress;
		var userAgent = request.UserAgent;

		var userDto = userProjection.ProjectionFunc(user);
		var token = tokenGenerator.CreateToken(
			userDto,
			deviceId,
			roles,
			permissions);
		var refreshToken = tokenGenerator.CreateRefreshToken();

		await userTokenService.AddToken(
			refreshToken,
			user.Id,
			TokenType.RefreshToken,
			DateTime.UtcNow.AddMonths(1),
			ip,
			userAgent,
			deviceId,
			[],
			cancellationToken);

		user.Login(ip?.ToString(), userAgent);

		return new LoginResult(
			token,
			refreshToken,
			deviceId);
	}

	private string GenerateDeviceId()
	{
		var bytes = RandomNumberGenerator.GetBytes(32);
		return Convert.ToHexString(bytes);
	}
}
