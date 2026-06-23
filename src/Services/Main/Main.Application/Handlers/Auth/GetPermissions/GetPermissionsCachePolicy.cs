using Application.Common.Interfaces.Cqrs;
using Localization.Abstractions.Interfaces;

namespace Main.Application.Handlers.Auth.GetPermissions;

public class GetPermissionsCachePolicy(IScopedStringLocalizer stringLocalizer) : ICachePolicy<GetPermissionsQuery>
{
    public TimeSpan TimeToLive => TimeSpan.FromDays(1);
    public IReadOnlyCollection<string>? Tags => null;
    public string? BaseTag => null;

    public string GetCacheKey(GetPermissionsQuery request)
        => $"list-permissions:{stringLocalizer.Locale}";
}