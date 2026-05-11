using Main.Enums;

namespace Main.Abstractions.Extensions;

public static class RoleExtensions
{
    public static string ToNormalized(this Role role)
    {
        return role.ToString().ToUpperInvariant();
    }
}