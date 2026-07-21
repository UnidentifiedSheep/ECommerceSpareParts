using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Auth;

public class Role : AuditableEntity<Role, string>, ILinqEntity<Role, string>
{
    private readonly List<RolePermission> _rolePermissions = [];

    private Role() { }

    private Role(string name) { Name = RoleNames.Normalize(name); }

    public string Name { get; } = null!;

    public string? Description { get; private set; }
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions;

    public static Expression<Func<Role, string>> GetKeySelector() { return x => x.Name; }

    public static Expression<Func<Role, bool>> GetEqualityExpression(string key)
    {
        var normalized = RoleNames.Normalize(key);
        return x => x.Name == normalized;
    }

    public static Role Create(string name) { return new Role(name); }

    public void SetDescription(string? description)
    {
        Description = description.NullIfWhiteSpace()
            ?
            .EnsureMaxLength(255, "role.description.max.length");
    }

    public void AddPermission(string name)
    {
        name = name.Trim();
        if (_rolePermissions.Any(x => x.PermissionName == name)) return;

        _rolePermissions.Add(RolePermission.Create(Name, name));
    }

    public void RemovePermission(string name)
    {
        var first = _rolePermissions.FirstOrDefault(x => x.PermissionName == name.Trim());
        if (first != null) _rolePermissions.Remove(first);
    }

    public override string GetId() { return Name; }
}