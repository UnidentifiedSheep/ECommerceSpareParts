using BulkValidation.Core.Attributes;

namespace Main.Entities;

public partial class Permission
{
    [Validate]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
