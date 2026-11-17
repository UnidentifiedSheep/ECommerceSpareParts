using System.Net;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Exceptions.Base;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Main.Modules.Identity.Application.Dtos.Users;
using Main.Modules.Identity.Application.Extensions;
using Main.Modules.Identity.Application.Extensions.QueryExtensions;
using Main.Modules.Identity.Application.Interfaces.Persistence;
using Main.Modules.Identity.Application.Interfaces.Services;
using Main.Modules.Identity.Application.Validators;
using Security.Core.Interfaces;
using Role = Enums.Role;

namespace Main.Modules.Identity.Application.Handlers.Auth;

[Transactional]
[AutoSave]
public record LoginCommand(
	string Login,
	string Password,
	IPAddress? IpAddress,
	string? UserAgent) : ICommand<LoginResult>;

public record LoginResult(string Token, string RefreshToken);

public class LoginHandler(
	IEmailValidator emailValidator,
	IPasswordManager passwordManager,
	IUserRepository userRepository,
	IJwtGenerator tokenGenerator,
	IUnitOfWork unitOfWork,
	IValueHasher hasher,
	IReadRepository<User, Guid> userReadRepository,
	IProjectionProvider<User, UserDto> userProjection) : ICommandHandler<LoginCommand, LoginResult>
{
	public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
	{
		var user = await userRepository.GetUserByLoginAsync(
			request.Login,
			emailValidator.IsValidEmail(request.Login),
			Criteria<User>
				.New()
				.WhereDoesNotHaveRole(Role.System)
				.Include(x => x.UserInfo)
				.ForUpdate()
				.Track()
				.Build(),
			cancellationToken) ?? throw new WrongCredentialsException(null, null);

		if (user.UserInfo == null)
			throw new InternalServerException("User exists, but unable to get user info.");
		if (!passwordManager.VerifyHashedPassword(user.PasswordHash, request.Password))
			throw new WrongCredentialsException(null, null);

		var (roles, permissions) =
			await userReadRepository.GetUserRolesAndPermissionsAsync(user.Id, cancellationToken) ??
			throw new UserNotFoundException(user.Id);

		var userDto = userProjection.ProjectionFunc(user);
		var token = tokenGenerator.CreateToken(
			userDto,
			roles,
			permissions);

		var refreshToken = tokenGenerator.CreateRefreshToken();
		var userToken = UserToken.CreateRefreshToken(user.Id, hasher.Hash(refreshToken));
		await unitOfWork.AddAsync(userToken, cancellationToken);

		user.Login(request.IpAddress, request.UserAgent);

		return new LoginResult(token, refreshToken);
	}
}
