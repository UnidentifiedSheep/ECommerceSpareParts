using Main.Entities.Auth;
using Role = Main.Enums.Role;

namespace Main.Application.Extensions;

public static class RoleExtensions
{
    public static string ToNormalizedRole(this Role role)
    {
        return RoleNames.Normalize(role.ToString());
    }
    
    public static string ToNormalizedRole(this string role)
    {
        return RoleNames.Normalize(role);
    }
}