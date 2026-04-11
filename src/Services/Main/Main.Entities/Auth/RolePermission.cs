namespace Main.Entities.Auth;

public class RolePermission
{
    public string RoleName { get; set; } = null!;
    public string PermissionName { get; set; } = null!;

    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}