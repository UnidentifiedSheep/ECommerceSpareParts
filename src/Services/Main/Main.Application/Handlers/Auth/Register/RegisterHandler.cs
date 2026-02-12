using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Dtos.Emails;
using Main.Abstractions.Dtos.Users;
using Main.Application.Handlers.Users.CreateUser;
using Main.Enums;
using MediatR;

namespace Main.Application.Handlers.Auth.Register;

[Transactional]
public record RegisterCommand(string Email, string UserName, string Password, string Name, string Surname)
    : ICommand<Unit>;

internal class RegisterHandler(IMediator mediator) : ICommandHandler<RegisterCommand, Unit>
{
    public async Task<Unit> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userInfo = new UserInfoDto
        {
            Name = request.Name,
            Surname = request.Surname,
            IsSupplier = false
        };
        var email = new EmailDto
        {
            Email = request.Email,
            IsConfirmed = false,
            IsPrimary = true,
            Type = EmailType.Unknown
        };
        var command = new CreateUserCommand(request.UserName, request.Password, userInfo,
            [email], [], ["Member"]);
        await mediator.Send(command, cancellationToken);
        return Unit.Value;
    }
}