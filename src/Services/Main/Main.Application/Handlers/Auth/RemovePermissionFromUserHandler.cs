using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.User;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using MediatR;

namespace Main.Application.Handlers.Auth;

[Diagnostics(maxExecutionTimeMs: 150)]
[Transactional]
[AutoSave]
public record RemovePermissionFromUserCommand(Guid UserId, string PermissionName) : ICommand;

public class RemovePermissionFromUserHandler(
    IRepository<UserPermission, (Guid, string)> repository,
    IUnitOfWork unitOfWork,
    IIntegrationEventScope interfaceScope
) : ICommandHandler<RemovePermissionFromUserCommand>
{
    public async Task<Unit> Handle(
        RemovePermissionFromUserCommand request,
        CancellationToken cancellationToken)
    {
        var userPermission = await repository.GetById(
                                 (request.UserId, request.PermissionName),
                                 cancellationToken)
                             ?? throw new UserPermissionNotFound(request.UserId, request.PermissionName);
        unitOfWork.Remove(userPermission);

        interfaceScope.Add(
            new UserUpdatedEvent
            {
                UserId = request.UserId
            });

        return Unit.Value;
    }
}