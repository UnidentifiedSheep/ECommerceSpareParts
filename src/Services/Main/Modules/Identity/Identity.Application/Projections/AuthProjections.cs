using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Locan.Core.Interfaces.Localizers;
using Locan.Core.LocalizableMessages;
using Main.Entities.Auth;
using Main.Modules.Identity.Application.Dtos.Auth;

namespace Main.Modules.Identity.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class RoleDtoProjectionProvider(IContextualLocalizer localizer)
	: ProjectionProviderBase<Role, RoleDto>
{
	public override Expression<Func<Role, RoleDto>> Projection { get; } = x => new RoleDto
	{
		SystemName = x.Name,
		LocalizedName = GetName(localizer, x.Name),
		Description = x.Description,
		WhoCreated = x.WhoCreated,
		WhoUpdated = x.WhoUpdated,
		CreatedAt = x.CreatedAt,
		UpdatedAt = x.UpdatedAt
	};

	private static string? GetName(IContextualLocalizer localizer, string name)
		=> localizer.TryGet(new LocalizableMessage($"role.{name}.name"), out var value) ? value : null;
}
