using Main.Entities.User;
using Main.Enums.Balances;
using Role = Main.Enums.Role;
using RoleExtensions = Main.Application.Extensions.RoleExtensions;

namespace Main.Application.Extensions.Entities;

public static class UserExtensions
{
    public static TransactionPartyType GetPartyType(this User user)
    {
        return user.HasSystem()
            ? TransactionPartyType.System
            : TransactionPartyType.User;
    }

    public static bool HasSystem(this User user)
    {
        return user.HasRole(Role.System);
    }

    public static bool HasRole(this User user, Role role)
    {
        return user.HasRole(RoleExtensions.ToNormalizedRole(role));
    }

    public static bool HasRole(this User user, string role)
    {
        var normalizedRole = RoleExtensions.ToNormalizedRole(role);
        return user.Roles.Any(z => z.RoleName == normalizedRole);
    }
}
