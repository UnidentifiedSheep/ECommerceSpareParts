using Enums;
using Microsoft.AspNetCore.Authorization;

namespace Security.Authorization;

public sealed class RoleRequirement : IAuthorizationRequirement
{
    public RoleRequirement(
        IEnumerable<string> roles,
        AuthorizationMatch match)
    {
        ArgumentNullException.ThrowIfNull(roles);

        Roles = roles
            .Select(AuthorizationValueNormalizer.NormalizeRole)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (Roles.Count == 0)
            throw new ArgumentException("At least one role is required.", nameof(roles));

        Match = match;
    }
    
    public RoleRequirement(
        IEnumerable<PermissionCodes> roles,
        AuthorizationMatch match) : this(roles.Select(x => x.ToString()), match) { }

    public IReadOnlyList<string> Roles { get; }
    public AuthorizationMatch Match { get; }
}
