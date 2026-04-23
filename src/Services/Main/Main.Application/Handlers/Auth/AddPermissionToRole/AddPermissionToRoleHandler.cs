using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Exceptions.Auth;
using Main.Entities.Auth;
using Main.Entities.Auth.ValueObjects;
using MediatR;

namespace Main.Application.Handlers.Auth.AddPermissionToRole;

[AutoSave]
[Transactional]
public record AddPermissionToRoleCommand(string RoleName, string PermissionName) : ICommand;

public class AddPermissionToRoleHandler(
    IRepository<Role, string> repository) : ICommandHandler<AddPermissionToRoleCommand>
{
    public async Task<Unit> Handle(AddPermissionToRoleCommand request, CancellationToken cancellationToken)
    {
        var criteria = Criteria<Role>.New()
            .Include(x => x.RolePermissions)
            .Track()
            .Where(x => x.Name.NormalizedValue == RoleName.ToNormalized(request.RoleName))
            .Build();
        
        var role = await repository.FirstOrDefaultAsync(criteria, cancellationToken)
            ?? throw new RoleNotFoundException(request.RoleName);
        
        role.AddPermission(request.PermissionName);
        
        return Unit.Value;
    }
}