using System.Linq.Expressions;
using Main.Application.Dtos.Auth;
using Main.Entities.Auth;

namespace Main.Application.Handlers.Projections;

public static class AuthProjections
{
    public static Expression<Func<Permission, PermissionDto>> ToPermissionDto =
        x => new PermissionDto
        {
            Name = x.Name,
            Description = x.Description,
        };

    public static Expression<Func<Role, RoleDto>> ToRoleDto =
        x => new RoleDto
        {
            Name = x.Name,
            Description = x.Description,
            WhoCreated = x.WhoCreated,
            WhoUpdated = x.WhoUpdated,
            CreatedAt = x.UpdatedAt,
            UpdatedAt = x.UpdatedAt
        };
}