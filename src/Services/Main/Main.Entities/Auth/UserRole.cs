using Domain;

namespace Main.Entities.Auth;

public class UserRole : AuditableEntity<UserRole, (Guid, string)>
{
    public Guid UserId { get; private set; }

    public string RoleName { get; private set; } = null!;

    public Role Role { get; private set; } = null!;

    private UserRole() {}

    private UserRole(Guid userId, string roleName)
    {
        UserId = userId;
        RoleName = ValueObjects.RoleName.ToNormalized(roleName);
    }

    public static UserRole Create(Guid userId, string roleName)
    {
        return new UserRole(userId, roleName);
    }
    
    public override (Guid, string) GetId() => (UserId, RoleName);
}