using Domain.Extensions;

namespace Main.Entities.Auth;

public static class RoleNames
{
    public static string Normalize(string name)
    {
        var value = name
            .Trim()
            .EnsureMinLength(3, "role.name.min.length")
            .EnsureMaxLength(24, "role.name.max.length");

        return value.ToUpperInvariant();
    }
}