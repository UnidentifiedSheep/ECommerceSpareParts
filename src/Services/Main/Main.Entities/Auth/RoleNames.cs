using Domain.Validation;

namespace Main.Entities.Auth;

public static class RoleNames
{
	public static string Normalize(string name)
	{
		var value = name
			.Trim()
			.EnsureMinLength(3, RoleNameMinLengthMessage.Instance)
			.EnsureMaxLength(24, RoleNameMaxLengthMessage.Instance);

		return value.ToUpperInvariant();
	}
}
