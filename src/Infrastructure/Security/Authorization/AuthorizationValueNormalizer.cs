using Extensions;

namespace Security.Authorization;

public static class AuthorizationValueNormalizer
{
    public static string NormalizePermission(string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        return permission.Trim().ToNormalizedPermission();
    }

    public static string NormalizeRole(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        return role.Trim().ToUpperInvariant();
    }
}
