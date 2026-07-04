using Domain.Extensions;

namespace Main.Entities.Auth;

public static class RoleNames
{
    public static string Normalize(string name)
    {
        var value = name
            .Trim()
            .AgainstTooShort(3, "role.name.min.length")
            .AgainstTooLong(24, "role.name.max.length");

        return value.ToUpperInvariant();
    }
}