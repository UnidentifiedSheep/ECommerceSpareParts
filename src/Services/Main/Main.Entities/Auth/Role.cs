using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Domain.Extensions;
using Main.Entities.Auth.ValueObjects;

namespace Main.Entities.Auth;

public class Role : AuditableEntity<Role, RoleName>, ILinqEntity<Role, RoleName>
{
    private readonly List<RolePermission> _rolePermissions = [];

    private Role()
    {
    }

    private Role(RoleName name)
    {
        Name = name;
    }

    public RoleName Name { get; } = null!;

    public string? Description { get; private set; }
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions;

    public static Role Create(RoleName name)
    {
        return new Role(name);
    }

    public void SetDescription(string? description)
    {
        Description = description.NullIfWhiteSpace()?
            .AgainstTooLong(255, "role.description.max.length");
    }

    public void AddPermission(string name)
    {
        name = name.Trim();
        if (_rolePermissions.Any(x => x.PermissionName == name.Trim()))
            return;

        _rolePermissions.Add(RolePermission.Create(Name.Value, name));
    }

    public void RemovePermission(string name)
    {
        var first = _rolePermissions.FirstOrDefault(x => x.PermissionName == name.Trim());
        if (first != null) _rolePermissions.Remove(first);
    }

    public override RoleName GetId()
    {
        return Name;
    }

    public static Expression<Func<Role, bool>> GetEqualityExpression(RoleName key)
        => x => x.Name == key;
}