using System.Linq.Expressions;
using Domain;

namespace Main.Entities.Auth;

public class UserPermission : AuditableEntity<UserPermission, (Guid, string)>
{
    private UserPermission()
    {
    }

    private UserPermission(Guid userId, string permission)
    {
        UserId = userId;
        Permission = permission;
    }

    public Guid UserId { get; }

    public string Permission { get; } = null!;

    public static UserPermission Create(Guid userId, string permission)
    {
        return new UserPermission(userId, permission);
    }

    public override (Guid, string) GetId()
    {
        return (UserId, Permission);
    }

    public override Expression<Func<UserPermission, bool>> GetEqualityExpression((Guid, string) key)
        => x => x.UserId == key.Item1 && x.Permission == key.Item2;
}