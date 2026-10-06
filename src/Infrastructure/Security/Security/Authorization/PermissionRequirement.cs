using Enums;
using Microsoft.AspNetCore.Authorization;

namespace Security.Authorization;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
	public PermissionRequirement(IEnumerable<string> permissions, AuthorizationMatch match)
	{
		ArgumentNullException.ThrowIfNull(permissions);

		Permissions = permissions
			.Select(AuthorizationValueNormalizer.NormalizePermission)
			.Distinct(StringComparer.Ordinal)
			.ToArray();

		if (Permissions.Count == 0)
			throw new ArgumentException("At least one permission is required.", nameof(permissions));

		Match = match;
	}

	public PermissionRequirement(IEnumerable<PermissionCodes> permissions, AuthorizationMatch match) : this(
		permissions.Select(z => z.ToString()),
		match)
	{
	}

	public IReadOnlyList<string> Permissions { get; }

	public AuthorizationMatch Match { get; }
}
