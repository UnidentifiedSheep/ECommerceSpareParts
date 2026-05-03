using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Auth;
using Main.Entities.Auth;
using Main.Entities.Auth.ValueObjects;
using Main.Entities.Exceptions.Auth;
using MediatR;

namespace Main.Application.Handlers.Auth.AddPermissionToRole;

[Transactional, AutoSave]
public record AddPermissionToRoleCommand(string RoleName, string PermissionName) : ICommand;

public class AddPermissionToRoleHandler(
    IRepository<Role, string> repository,
    IIntegrationEventScope integrationEventScope) : ICommandHandler<AddPermissionToRoleCommand>
{
    public async Task<Unit> Handle(AddPermissionToRoleCommand request, CancellationToken cancellationToken)
    {
        var criteria = Criteria<Role>.New()
            .Include(x => x.RolePermissions)
            .Track()
            .Where(x => x.Name.Value == RoleName.ToNormalized(request.RoleName))
            .Build();
        
        var role = await repository.FirstOrDefaultAsync(criteria, cancellationToken)
            ?? throw new RoleNotFoundException(request.RoleName);
        
        role.AddPermission(request.PermissionName);
        
        integrationEventScope.Add(new RoleUpdatedEvent
        {
            RoleName = request.RoleName
        });
        
        return Unit.Value;
    }
}