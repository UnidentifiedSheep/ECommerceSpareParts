using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.User;
using Main.Entities.Auth;
using MediatR;

namespace Main.Application.Handlers.Users.AddPermissionToUser;

[AutoSave]
[Transactional]
public record AddPermissionToUserCommand(Guid UserId, string PermissionName) : ICommand;

public class AddPermissionToUserHandler(
    IUnitOfWork unitOfWork,
    IIntegrationEventScope interfaceScope
    ) : ICommandHandler<AddPermissionToUserCommand>
{
    public async Task<Unit> Handle(AddPermissionToUserCommand request, CancellationToken cancellationToken)
    {
        UserPermission model = UserPermission.Create(request.UserId, request.PermissionName);

        await unitOfWork.AddAsync(model, cancellationToken);
        interfaceScope.Add(new UserUpdatedEvent
        {
            UserId = request.UserId,
        });
        return Unit.Value;
    }
}