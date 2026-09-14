using System.Globalization;
using Application.Common.Interfaces.Cqrs;
using Locan.Core.Interfaces.Localizers;

namespace Main.Application.Handlers.Auth.GetPermissions;

public class GetPermissionsCachePolicy : ICachePolicy<GetPermissionsQuery>
{
	public TimeSpan TimeToLive => TimeSpan.FromDays(1);

	public IReadOnlyCollection<string>? Tags => null;

	public string? BaseTag => null;

	public string GetCacheKey(GetPermissionsQuery request) => $"list-permissions:{CultureInfo.CurrentUICulture}";
}
