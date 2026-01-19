using BulkValidation.Core.Attributes;

namespace Main.Entities;

public partial class Role
{
    [Validate]
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
    [Validate]

    public string NormalizedName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsSystem { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<Permission> PermissionNames { get; set; } = new List<Permission>();
}
