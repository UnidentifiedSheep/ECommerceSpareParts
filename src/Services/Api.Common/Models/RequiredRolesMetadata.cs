namespace Api.Common.Models;

public sealed class RequiredRolesMetadata
{
    public RequiredRolesMetadata(string[] roles, bool requireAll)
    {
        Roles = roles;
        RequireAll = requireAll;
    }

    public string[] Roles { get; }
    public bool RequireAll { get; }
}
