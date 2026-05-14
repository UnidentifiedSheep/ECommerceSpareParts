using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Enums;
using Extensions;

namespace Main.Entities.Auth;

public class Permission : AuditableEntity<Permission, string>
{
    private Permission()
    {
    }

    public Permission(PermissionCodes name, string? description = null)
    {
        Name = ToNormalizedPermission(name);
        Description = description;
    }

    [Validate]
    public string Name { get; } = null!;

    public string? Description { get; private set; }

    public override string GetId()
    {
        return Name;
    }

    public override Expression<Func<Permission, bool>> GetEqualityExpression(string key)
        => x => x.Name == key;

    public static string ToNormalizedPermission(PermissionCodes permission)
    {
        return permission.ToString().ToNormalizedPermission();
    }
}