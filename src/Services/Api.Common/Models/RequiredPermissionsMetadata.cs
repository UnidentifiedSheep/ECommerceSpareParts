namespace Api.Common.Models;

public sealed class RequiredPermissionsMetadata
{
    public string[] Permissions { get; }
    public bool RequireAll { get; }

    public RequiredPermissionsMetadata(string[] permissions, bool requireAll)
    {
        Permissions = permissions;
        RequireAll = requireAll;
    }
}