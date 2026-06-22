using System.Linq.Expressions;
using Localization.Abstractions.Interfaces;
using Main.Application.Dtos.Auth;
using Main.Entities.Auth;

namespace Main.Application.Projections;

public static class AuthProjections
{
    public static Expression<Func<Role, RoleDto>> ToRoleDto(IScopedStringLocalizer localizer) =>
        x => new RoleDto
        {
            SystemName = x.Name,
            LocalizedName = localizer.GetOrDefault($"role.{x.Name}.name") ?? x.Name,
            Description = x.Description,
            WhoCreated = x.WhoCreated,
            WhoUpdated = x.WhoUpdated,
            CreatedAt = x.UpdatedAt,
            UpdatedAt = x.UpdatedAt
        };
}