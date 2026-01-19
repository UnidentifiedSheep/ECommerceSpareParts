using Application.Common.Interfaces;
using Core.Attributes;
using Core.Extensions;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Permissions;
using Exceptions.Exceptions.Roles;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using MediatR;

namespace Main.Application.Handlers.Roles.AddPermissionToRole;

[Transactional]
public record AddPermissionToRoleCommand(Guid RoleId, string PermissionName) : ICommand;

public class AddPermissionToRoleHandler(IUnitOfWork unitOfWork, 
    IRoleRepository roleRepository, IPermissionRepository permissionRepository) : ICommandHandler<AddPermissionToRoleCommand>
{
    public async Task<Unit> Handle(AddPermissionToRoleCommand request, CancellationToken cancellationToken)
    {
        Role role = await roleRepository.GetRoleAsync(request.RoleId, true, cancellationToken)
            ?? throw new RoleNotFoundException(request.RoleId);
        Permission permission = await permissionRepository
                                    .GetPermissionAsync(request.PermissionName.ToNormalized(), true, cancellationToken)
            ?? throw new PermissionNotFoundException(request.PermissionName);
        role.PermissionNames.Add(permission);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}