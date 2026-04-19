using BulkValidation.Core.Attributes;
using Domain;
using Enums;
using Extensions;

namespace Main.Entities.Auth;

public class Permission : AuditableEntity<Permission, string>
{
    [Validate]
    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }
    public override string GetId() => Name;
    
    private Permission() {}
    
    public Permission(PermissionCodes name, string? description = null)
    {
        Name = ToNormalizedPermission(name);
        Description = description;
    }
    
    public static string ToNormalizedPermission(PermissionCodes permission)
    {
        return permission.ToString().ToUpperInvariant().Replace('_', '.');
    }
}