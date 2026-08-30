using Application.Common.Interfaces.Cqrs;
using Enums;
using Localization.Abstractions.Interfaces;
using Main.Application.Dtos.Auth;
using Main.Entities.Auth;

namespace Main.Application.Handlers.Auth.GetPermissions;

public record GetPermissionsQuery : IQuery<GetPermissionsResult>;

public record GetPermissionsResult(IReadOnlyList<PermissionDto> Permissions);

public class GetPermissionsHandler(IContextualStringLocalizer localizer)
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
				Name = localizer.Get(Permission.GetLocalizationNameKey(x)),
				Description = localizer.Get(Permission.GetLocalizationDescriptionKey(x))
			})
			.ToArray();

		return new GetPermissionsResult(permissions);
	}
}
