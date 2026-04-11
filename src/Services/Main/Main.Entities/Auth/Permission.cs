using BulkValidation.Core.Attributes;
using Domain;
using Enums;
using Extensions;

namespace Main.Entities;

public class Permission : AuditableEntity<Permission, string>
{
    public Permission()
    {
    }

    public Permission(PermissionCodes name, string? description = null)
    {
        Name = name.ToNormalizedPermission();
        Description = description;
    }

    [Validate]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public override string GetId() => Name;
}