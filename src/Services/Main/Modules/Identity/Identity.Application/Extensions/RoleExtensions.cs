using Main.Entities.Auth;
using Role = Enums.Role;

namespace Main.Modules.Identity.Application.Extensions;

public static class RoleExtensions
{
	public static string ToNormalizedRole(this Role role) => RoleNames.Normalize(role.ToString());

	public static string ToNormalizedRole(this string role) => RoleNames.Normalize(role);
}
