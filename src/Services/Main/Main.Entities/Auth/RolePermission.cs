using System.Linq.Expressions;
using Domain;
using Main.Entities.Auth.ValueObjects;

namespace Main.Entities.Auth;

public class RolePermission : Entity<RolePermission, (string, string)>
{
    private RolePermission()
    {
    }

    private RolePermission(RoleName roleName, string permissionName)
    {
        RoleName = roleName;
        PermissionName = permissionName;
    }

    public RoleName RoleName { get; } = null!;
    public string PermissionName { get; } = null!;

    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;

    public static RolePermission Create(RoleName roleName, string permissionName)
    {
        return new RolePermission(roleName, permissionName);
    }

    public override (string, string) GetId()
    {
        return (RoleName, PermissionName);
    }

    public override Expression<Func<RolePermission, bool>> GetEqualityExpression((string, string) key)
        => x => x.RoleName == key.Item1 && x.PermissionName == key.Item2;
}