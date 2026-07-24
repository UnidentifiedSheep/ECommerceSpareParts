using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Localization.Abstractions.Interfaces;
using Main.Application.Dtos.Auth;
using Main.Entities.Auth;

namespace Main.Application.Projections;

public sealed class RoleDtoProjectionProvider(
    IScopedStringLocalizer localizer)
    : IScopedProjectionProvider<Role, RoleDto>
{
    public Expression<Func<Role, RoleDto>> Projection { get; } =
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
