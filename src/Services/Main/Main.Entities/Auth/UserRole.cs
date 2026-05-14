using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Main.Entities.Auth.ValueObjects;

namespace Main.Entities.Auth;

public class UserRole : AuditableEntity<UserRole, (Guid, string)>, ILinqEntity<UserRole, (Guid, string)>
{
    private UserRole()
    {
    }

    private UserRole(Guid userId, RoleName roleName)
    {
        UserId = userId;
        RoleName = roleName;
    }

    public Guid UserId { get; }

    public RoleName RoleName { get; } = null!;

    public Role Role { get; private set; } = null!;

    public static UserRole Create(Guid userId, string roleName)
    {
        return new UserRole(userId, roleName);
    }

    public override (Guid, string) GetId()
    {
        return (UserId, RoleName);
    }

    public static Expression<Func<UserRole, bool>> GetEqualityExpression((Guid, string) key)
        => x => x.UserId == key.Item1 && x.RoleName == key.Item2;
}