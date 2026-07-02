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

namespace Main.Application.Handlers.Auth.RemoveRoleFromUser;

[Diagnostics(maxExecutionTimeMs: 150)]
[Transactional]
[AutoSave]
public record RemoveRoleFromUserCommand(Guid UserId, string RoleName) : ICommand;

public class RemoveRoleFromUserHandler(
    IRepository<UserRole, (Guid, string)> repository,
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope
) : ICommandHandler<RemoveRoleFromUserCommand>
{
    public async Task<Unit> Handle(
        RemoveRoleFromUserCommand request,
        CancellationToken cancellationToken)
    {
        var userRole = await repository.GetById((request.UserId, request.RoleName), cancellationToken)
                       ?? throw new UserRoleNotFoundException(request.UserId, request.RoleName);

        unitOfWork.Remove(userRole);

        integrationEventScope.Add(
            new UserUpdatedEvent
            {
                UserId = request.UserId
            });

        return Unit.Value;
    }
}