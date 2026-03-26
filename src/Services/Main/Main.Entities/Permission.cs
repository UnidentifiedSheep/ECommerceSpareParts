using BulkValidation.Core.Attributes;
using Enums;
using Extensions;

namespace Main.Entities;

public class Permission
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

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}