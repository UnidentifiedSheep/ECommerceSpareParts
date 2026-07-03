using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Attributes;
using Contracts.User;
using Main.Entities.Auth;
using MediatR;

namespace Main.Application.Handlers.Auth.AddRoleToUser;

[Diagnostics(maxExecutionTimeMs: 400)]
[Transactional]
[AutoSave]
public record AddRoleToUserCommand(Guid UserId, string RoleName) : ICommand;

public class AddRoleToUserHandler(
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope
) : ICommandHandler<AddRoleToUserCommand>
{
    public async Task<Unit> Handle(AddRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var userRole = UserRole.Create(request.UserId, request.RoleName);
        await unitOfWork.AddAsync(userRole, cancellationToken);
        integrationEventScope.Add(
            new UserUpdatedEvent
            {
                UserId = userRole.UserId
            });
        return Unit.Value;
    }
}