using BulkValidation.Core.Attributes;
using Domain;

namespace Main.Entities;

public class Role : AuditableEntity<Role, string>
{
    public string Name { get; set; } = null!;

    [Validate]
    public string NormalizedName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public override string GetId() => NormalizedName;
}