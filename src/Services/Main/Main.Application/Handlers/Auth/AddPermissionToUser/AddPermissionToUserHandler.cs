using Abstractions.Interfaces.Persistence;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Contracts.User;
using Main.Entities.Auth;
using MediatR;

namespace Main.Application.Handlers.Auth.AddPermissionToUser;

[Transactional]
[AutoSave]
public record AddPermissionToUserCommand(Guid UserId, string PermissionName) : ICommand;

public class AddPermissionToUserHandler(
    IUnitOfWork unitOfWork,
    IIntegrationEventScope interfaceScope
) : ICommandHandler<AddPermissionToUserCommand>
{
    public async Task<Unit> Handle(AddPermissionToUserCommand request, CancellationToken cancellationToken)
    {
        var model = UserPermission.Create(request.UserId, request.PermissionName);

        await unitOfWork.AddAsync(model, cancellationToken);
        interfaceScope.Add(new UserUpdatedEvent
        {
            UserId = request.UserId
        });
        return Unit.Value;
    }
}