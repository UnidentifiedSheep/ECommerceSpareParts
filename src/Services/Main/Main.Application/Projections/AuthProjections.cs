using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Locan.Core.Interfaces.Localizers;
using Locan.Core.LocalizableMessages;
using Main.Application.Dtos.Auth;
using Main.Entities.Auth;

namespace Main.Application.Projections;

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
		CreatedAt = x.UpdatedAt,
		UpdatedAt = x.UpdatedAt
	};

	private static string? GetName(IContextualLocalizer localizer, string name) =>
		localizer.TryGet(new LocalizableMessage($"role.{name}.name"), out var value) ? value : null;
}
