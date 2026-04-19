using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Entities.Auth;
using MediatR;

namespace Main.Application.Handlers.Roles.CreateRole;

[AutoSave]
[Transactional]
public record CreateRoleCommand(string Name, string? Description) : ICommand;

public class CreateRoleHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRoleCommand>
{
    public async Task<Unit> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = Role.Create(request.Name);
        role.SetDescription(request.Description);

        await unitOfWork.AddAsync(role, cancellationToken);
        return Unit.Value;
    }
}