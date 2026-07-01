using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Auth;

public class RolePermission : Entity<RolePermission, (string, string)>,
    ILinqEntity<RolePermission, (string, string)>
{
    private RolePermission() { }

    private RolePermission(string roleName, string permissionName)
    {
        RoleName = RoleNames.Normalize(roleName);
        PermissionName = permissionName;
    }

    public string RoleName { get; } = null!;
    public string PermissionName { get; } = null!;
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;

    public static Expression<Func<RolePermission, (string, string)>> GetKeySelector()
    {
        return x => ValueTuple.Create(x.RoleName, x.PermissionName);
    }

    public static Expression<Func<RolePermission, bool>> GetEqualityExpression((string, string) key)
    {
        var normalized = RoleNames.Normalize(key.Item1);
        return x => x.RoleName == normalized && x.PermissionName == key.Item2;
    }

    public static RolePermission Create(string roleName, string permissionName)
    {
        return new RolePermission(roleName, permissionName);
    }

    public override (string, string) GetId() { return (RoleName, PermissionName); }
}