using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
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
    IUnitOfWork unitOfWork
) : ICommandHandler<RemoveRoleFromUserCommand>
{
    public async Task<Unit> Handle(
        RemoveRoleFromUserCommand request,
        CancellationToken cancellationToken)
    {
        var userRole = await repository.GetById((request.UserId, request.RoleName), cancellationToken)
                       ?? throw new UserRoleNotFoundException(request.UserId, request.RoleName);

        unitOfWork.Remove(userRole);

        return Unit.Value;
    }
}
