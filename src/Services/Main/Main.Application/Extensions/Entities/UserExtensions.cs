using Main.Entities.User;
using Role = Enums.Role;

namespace Main.Application.Extensions.Entities;

public static class UserExtensions
{

	public static bool HasRole(this User user, Role role) => user.HasRole(role.ToNormalizedRole());

	public static bool HasRole(this User user, string role)
	{
		var normalizedRole = role.ToNormalizedRole();
		return user.Roles.Any(z => z.RoleName == normalizedRole);
	}
}
