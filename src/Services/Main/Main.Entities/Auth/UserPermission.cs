using Domain;

namespace Main.Entities.Auth;

public class UserPermission : AuditableEntity<UserPermission, (Guid, string)>
{
    public Guid UserId { get; private set; }

    public string Permission { get; private set; } = null!;

    private UserPermission() {}

    private UserPermission(Guid userId, string permission)
    {
        UserId = userId;
        Permission = permission;
    }

    public static UserPermission Create(Guid userId, string permission)
    {
        return new UserPermission(userId, permission);
    }
    
    public override (Guid, string) GetId() => (UserId, Permission);
}