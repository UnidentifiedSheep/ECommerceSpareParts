using Application.Interfaces;
using Core.Entities;
using Core.Extensions;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Roles;
using MediatR;

namespace Application.Handlers.Roles.CreateRole;

public record CreateRoleCommand(string Name, string? Description) : ICommand;

public class CreateRoleHandler(IRoleRepository roleRepository, IUnitOfWork unitOfWork) : ICommandHandler<CreateRoleCommand>
{
    public async Task<Unit> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleName = request.Name.Trim();
        await ValidateAsync(roleName, cancellationToken);
        var newRole = new Role
        {
            Name = roleName,
            NormalizedName = roleName.ToNormalized(),
            Description = request.Description,
            IsSystem = false
        };
        
        await unitOfWork.AddAsync(newRole, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task ValidateAsync(string roleName, CancellationToken cancellationToken = default)
    {
        if (await roleRepository.RoleExistsAsync(roleName, cancellationToken))
            throw new RoleAlreadyExistsException(roleName);
    }
}