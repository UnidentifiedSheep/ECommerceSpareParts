using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Auth;

public class UserRole : AuditableEntity<UserRole, (Guid, string)>, ILinqEntity<UserRole, (Guid, string)>
{
    private UserRole()
    {
    }

    private UserRole(Guid userId, string roleName)
    {
        UserId = userId;
        RoleName = RoleNames.Normalize(roleName);
    }

    public Guid UserId { get; }
    public string RoleName { get; } = null!;
    public Role Role { get; private set; } = null!;

    public static Expression<Func<UserRole, (Guid, string)>> GetKeySelector()
    {
        return x => ValueTuple.Create(x.UserId, x.RoleName);
    }

    public static Expression<Func<UserRole, bool>> GetEqualityExpression((Guid, string) key)
    {
        var normalized = RoleNames.Normalize(key.Item2);
        return x => x.UserId == key.Item1 && x.RoleName == normalized;
    }

    public static UserRole Create(Guid userId, string roleName)
    {
        return new UserRole(userId, roleName);
    }

    public override (Guid, string) GetId()
    {
        return (UserId, RoleName);
    }
}
