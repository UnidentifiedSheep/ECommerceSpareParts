using Abstractions.Interfaces.Validators;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Contracts.User;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions;
using MediatR;

namespace Main.Application.Handlers.Auth.ChangePassword;

[AutoSave]
[Transactional]
public record ChangePasswordCommand(
    Guid UserId,
    string PreviousPassword,
    string NewPassword
) : ICommand;

public class ChangePasswordHandler(
    IUserRepository userRepository,
    IPasswordManager passwordManager,
    IIntegrationEventScope integrationEventScope
) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(request.UserId, cancellationToken)
                   ?? throw new UserNotFoundException(request.UserId);

        if (!passwordManager.VerifyHashedPassword(user.PasswordHash, request.PreviousPassword))
            throw new WrongCredentialsException(null, request.PreviousPassword);

        user.SetPasswordHash(passwordManager.GetHashOfPassword(request.NewPassword));
        integrationEventScope.Add(
            new UserUpdatedEvent
            {
                UserId = request.UserId
            });

        return Unit.Value;
    }
}