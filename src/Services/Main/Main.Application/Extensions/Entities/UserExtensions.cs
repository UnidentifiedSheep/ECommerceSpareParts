using Main.Entities.User;
using Main.Enums.Auth;
using Role = Enums.Role;

namespace Main.Application.Extensions.Entities;

public static class UserExtensions
{
    public static UserPartyType GetPartyType(this User user)
    {
        return user.HasSystem()
            ? UserPartyType.System
            : UserPartyType.User;
    }

    public static bool HasSystem(this User user) { return user.HasRole(Role.System); }

    public static bool HasRole(this User user, Role role) { return user.HasRole(role.ToNormalizedRole()); }

    public static bool HasRole(this User user, string role)
    {
        var normalizedRole = role.ToNormalizedRole();
        return user.Roles.Any(z => z.RoleName == normalizedRole);
    }
}