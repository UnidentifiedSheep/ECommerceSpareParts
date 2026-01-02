using Main.Core.Permissions;

namespace Main.Core.Extensions;

public static class PermissionExtensions
{
    /// <summary>
    /// Replaces underscore with dot
    /// </summary>
    /// <param name="permissionCode"></param>
    /// <returns></returns>
    public static string ToPermissionName(this PermissionCodes permissionCode)
    {
        return permissionCode.ToString().Replace('_', '.');
    }
}