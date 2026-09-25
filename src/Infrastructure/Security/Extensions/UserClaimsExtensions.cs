using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Security.Authorization;

namespace Security.Extensions;

public static class UserClaimsExtensions
{
	public static ClaimsPrincipal? GetPrincipal(this IHttpContextAccessor accessor) =>
		accessor.HttpContext?.User;

	public static Guid? GetUserId(this ClaimsPrincipal principal)
	{
		var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		return Guid.TryParse(value, out var id) ? id : null;
	}

	public static IReadOnlySet<string> GetRoles(this ClaimsPrincipal principal) =>
		principal
			.FindAll(ClaimTypes.Role)
			.Select(x => x.Value)
			.Where(x => !string.IsNullOrWhiteSpace(x))
			.Select(AuthorizationValueNormalizer.NormalizeRole)
			.ToHashSet(StringComparer.Ordinal);

	public static IReadOnlySet<string> GetPermissions(this ClaimsPrincipal principal) =>
		principal
			.FindAll("permission")
			.Select(x => x.Value)
			.Where(x => !string.IsNullOrWhiteSpace(x))
			.Select(AuthorizationValueNormalizer.NormalizePermission)
			.ToHashSet(StringComparer.Ordinal);
}
