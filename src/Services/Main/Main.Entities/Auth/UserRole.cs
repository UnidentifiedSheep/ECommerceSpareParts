using Domain;
using Main.Entities.Auth.ValueObjects;

namespace Main.Entities.Auth;

public class UserRole : AuditableEntity<UserRole, (Guid, string)>
{
    public Guid UserId { get; private set; }

    public RoleName RoleName { get; private set; } = null!;

    public Role Role { get; private set; } = null!;

    private UserRole() {}

    private UserRole(Guid userId, RoleName roleName)
    {
        UserId = userId;
        RoleName = roleName;
    }

    public static UserRole Create(Guid userId, string roleName)
    {
        return new UserRole(userId, roleName);
    }
    
    public override (Guid, string) GetId() => (UserId, RoleName);
}