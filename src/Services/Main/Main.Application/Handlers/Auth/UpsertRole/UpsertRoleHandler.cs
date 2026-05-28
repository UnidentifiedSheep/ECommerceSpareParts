using Abstractions.Interfaces.Persistence;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Auth;
using MediatR;

namespace Main.Application.Handlers.Auth.UpsertRole;

[AutoSave]
[Transactional]
public record UpsertRoleCommand(string Name, string? Description) : ICommand;

public class UpsertRoleHandler(
    IUnitOfWork unitOfWork,
    IRepository<Role, string> repository)
    : ICommandHandler<UpsertRoleCommand>
{
    public async Task<Unit> Handle(UpsertRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await repository.GetById(request.Name, cancellationToken);
        if (role == null)
        {
            role = Role.Create(request.Name);
            await unitOfWork.AddAsync(role, cancellationToken);
        }

        role.SetDescription(request.Description);
        return Unit.Value;
    }
}
