using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Interfaces;
using Enums;
using Extensions;

namespace Main.Entities.Auth;

public class Permission : AuditableEntity<Permission, string>, ILinqEntity<Permission, string>
{
    private const string Prefix = "permissions.";
    private const string NamePostfix = ".name";
    private const string DescriptionPostfix = ".description";

    private Permission() { }

    public Permission(PermissionCodes name) { Name = ToNormalizedPermission(name); }

    [Validate]
    public string Name { get; } = null!;

    public static Expression<Func<Permission, string>> GetKeySelector() { return x => x.Name; }

    public static Expression<Func<Permission, bool>> GetEqualityExpression(string key)
    {
        return x => x.Name == key;
    }

    public override string GetId() { return Name; }

    public static string ToNormalizedPermission(PermissionCodes permission)
    {
        return permission.ToString().ToNormalizedPermission();
    }

    public static string GetLocalizationNameKey(PermissionCodes permission)
    {
        return Prefix + ToNormalizedPermission(permission).ToLowerInvariant() + NamePostfix;
    }

    public static string GetLocalizationDescriptionKey(PermissionCodes permission)
    {
        return Prefix + ToNormalizedPermission(permission).ToLowerInvariant() + DescriptionPostfix;
    }
}