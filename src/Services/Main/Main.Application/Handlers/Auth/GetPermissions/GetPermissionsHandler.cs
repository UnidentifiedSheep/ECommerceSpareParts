using Application.Common.Interfaces.Cqrs;
using Enums;
using Locan.Core.Interfaces.Localizers;
using Locan.Core.LocalizableMessages;
using Main.Application.Dtos.Auth;
using Main.Entities.Auth;

namespace Main.Application.Handlers.Auth.GetPermissions;

public record GetPermissionsQuery : IQuery<GetPermissionsResult>;

public record GetPermissionsResult(IReadOnlyList<PermissionDto> Permissions);

public class GetPermissionsHandler(IContextualLocalizer localizer)
	: IQueryHandler<GetPermissionsQuery, GetPermissionsResult>
{
	public async Task<GetPermissionsResult> Handle(
		GetPermissionsQuery request,
		CancellationToken cancellationToken)
	{
		var permissions = Enum
			.GetValues<PermissionCodes>()
			.Select(x => new PermissionDto
			{
				SystemName = Permission.ToNormalizedPermission(x),
				Name = localizer.Get(new LocalizableMessage(Permission.GetLocalizationNameKey(x))),
				Description = localizer.Get(new LocalizableMessage(Permission.GetLocalizationDescriptionKey(x)))
			})
			.ToArray();

		return new GetPermissionsResult(permissions);
	}
}
