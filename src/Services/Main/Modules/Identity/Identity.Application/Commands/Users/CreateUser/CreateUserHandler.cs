using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Main.Modules.Identity.Abstractions.Events;
using Main.Modules.Identity.Application.Dtos.Emails;
using Main.Modules.Identity.Application.Dtos.Users;
using Security.Core.Interfaces;
using Role = Enums.Role;

namespace Main.Modules.Identity.Application.Commands.Users.CreateUser;

[AutoSave]
[Transactional]
public record CreateUserCommand(
	string UserName,
	string Password,
	UserInfoDto UserInfo,
	IReadOnlyCollection<EmailDto> Emails,
	IReadOnlyCollection<UserPhoneDto> Phones,
	IReadOnlyCollection<string> Roles) : ICommand<CreateUserResult>;

public record CreateUserResult(Guid UserId);

public class CreateUserHandler(
	IUnitOfWork unitOfWork,
	IPasswordManager passwordManager,
	IDomainEventScope domainEventScope)
	: ICommandHandler<CreateUserCommand, CreateUserResult>
{
	public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
	{
		var passwordHash = passwordManager.GetHashOfPassword(request.Password);
		var user = User.Create(request.UserName, passwordHash);
		user.SetUserInfo(
			request.UserInfo.Name,
			request.UserInfo.Surname,
			request.UserInfo.Description);

		foreach (var role in request.Roles)
		{
			var normalizedRole = RoleNames.Normalize(role);
			if (normalizedRole == RoleNames.Normalize(nameof(Role.System)))
				throw new CantCreateSystemUserException();
			user.AddRole(normalizedRole);
		}

		foreach (var email in request.Emails)
			user.AddEmail(
				email.Email,
				email.Type,
				email.IsPrimary,
				email.IsConfirmed);

		foreach (var phone in request.Phones)
			user.AddUserPhone(
				phone.Number,
				phone.Type,
				phone.IsPrimary,
				phone.IsConfirmed);

		domainEventScope.Add(
			new UserRegistered(
				user.Id,
				request.UserInfo.Name,
				request.UserInfo.Surname));

		await unitOfWork.AddAsync(user, cancellationToken);
		return new CreateUserResult(user.Id);
	}
}
