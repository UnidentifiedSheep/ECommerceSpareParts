using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Interfaces;
using Enums;
using Extensions;

namespace Main.Entities.Auth;

public class Permission : AuditableEntity<Permission, string>, ILinqEntity<Permission, string>
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

    public static Expression<Func<Permission, string>> GetKeySelector()
    {
        return x => x.Name;
    }

    public static Expression<Func<Permission, bool>> GetEqualityExpression(string key)
    {
        return x => x.Name == key;
    }

    public override string GetId()
    {
        return Name;
    }

    public static string ToNormalizedPermission(PermissionCodes permission)
    {
        return permission.ToString().ToNormalizedPermission();
    }
}
