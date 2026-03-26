namespace Api.Common.Models;

public sealed class RequiredPermissionsMetadata
{
    public RequiredPermissionsMetadata(string[] permissions, bool requireAll)
    {
        Permissions = permissions;
        RequireAll = requireAll;
    }

    public string[] Permissions { get; }
    public bool RequireAll { get; }
}