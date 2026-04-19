using Domain;

namespace Main.Entities.Auth;

public class RolePermission : Entity<RolePermission, (string, string)>
{
    public string RoleName { get; private set; } = null!;
    public string PermissionName { get; private set; } = null!;

    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
    
    private RolePermission() {}

    private RolePermission(string roleName, string permissionName)
    {
        RoleName = roleName;
        PermissionName = permissionName;
    }

    public static RolePermission Create(string roleName, string permissionName)
    {
        return new RolePermission(roleName, permissionName);
    }
    
    public override (string, string) GetId() => (RoleName, PermissionName);
}