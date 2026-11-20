using Application.Common.Interfaces.Cqrs;
using Attributes;
using Enums;
using Main.Entities.Enums;
using Main.Modules.Identity.Application.Commands.Users.CreateUser;
using Main.Modules.Identity.Application.Dtos.Emails;
using Main.Modules.Identity.Application.Dtos.Users;
using MediatR;

namespace Main.Modules.Identity.Application.Commands.Auth.Register;

[Transactional]
public record RegisterCommand(
	string Email,
	string UserName,
	string Password,
	string Name,
	string Surname) : ICommand<Unit>;

internal class RegisterHandler(IMediator mediator) : ICommandHandler<RegisterCommand, Unit>
{
	public async Task<Unit> Handle(RegisterCommand request, CancellationToken cancellationToken)
	{
		var userInfo = new UserInfoDto
		{
			Name = request.Name, Surname = request.Surname
		};
		var email = new EmailDto
		{
			Email = request.Email,
			IsConfirmed = false,
			IsPrimary = true,
			Type = EmailType.Unknown
		};
		var command = new CreateUserCommand(
			request.UserName,
			request.Password,
			userInfo,
			[email],
			[],
			[nameof(Role.Member)]);
		await mediator.Send(command, cancellationToken);
		return Unit.Value;
	}
}
