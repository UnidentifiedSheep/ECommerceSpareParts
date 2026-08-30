using Abstractions.Interfaces;
using Security.Authorization;

namespace Api.Common.Extensions;

public static class UserContextExtensions
{
	public static bool ContainsRole(this IUserContext userContext, string role) =>
		userContext.Roles.Contains(AuthorizationValueNormalizer.NormalizeRole(role));

	public static bool ContainsRole(this IUserContext userContext, Enum role) =>
		userContext.Roles.Contains(AuthorizationValueNormalizer.NormalizeRole(role.ToString()));

	public static bool ContainsPermission(this IUserContext userContext, string permission) =>
		userContext.Permissions.Contains(AuthorizationValueNormalizer.NormalizePermission(permission));

	public static bool ContainsPermission(this IUserContext userContext, Enum permission) =>
		userContext.Permissions.Contains(
			AuthorizationValueNormalizer.NormalizePermission(permission.ToString()));
}
