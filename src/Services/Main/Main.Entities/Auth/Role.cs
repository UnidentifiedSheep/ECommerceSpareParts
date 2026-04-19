using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Main.Entities.Auth.ValueObjects;

namespace Main.Entities.Auth;

public class Role : AuditableEntity<Role, string>
{
    public RoleName Name { get; private set; } = null!;

    public string? Description { get; private set; }

    private List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions;
    
    private Role() {}

    private Role(RoleName name)
    {
        Name = name;
    }

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
        
        _rolePermissions.Add(RolePermission.Create(Name.NormalizedValue, name));
    }

    public void RemovePermission(string name)
    {
        var first = _rolePermissions.FirstOrDefault(x => x.PermissionName == name.Trim());
        if (first != null) _rolePermissions.Remove(first);
    }
    
    public override string GetId() => Name.NormalizedValue;
}