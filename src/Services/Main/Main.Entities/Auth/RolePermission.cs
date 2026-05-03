using Domain;
using Main.Entities.Auth.ValueObjects;

namespace Main.Entities.Auth;

public class RolePermission : Entity<RolePermission, (string, string)>
{
    public RoleName RoleName { get; private set; } = null!;
    public string PermissionName { get; private set; } = null!;

    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
    
    private RolePermission() {}

    private RolePermission(RoleName roleName, string permissionName)
    {
        RoleName = roleName;
        PermissionName = permissionName;
    }

    public static RolePermission Create(RoleName roleName, string permissionName)
    {
        return new RolePermission(roleName, permissionName);
    }
    
    public override (string, string) GetId() => (RoleName, PermissionName);
}