using Abstractions.Interfaces;

namespace Test.Common.Stubs;

public class UserContextMock : IUserContext
{
    public bool IsAuthenticated { get; private set; }
    public Guid UserId { get; private set; }

    private readonly HashSet<string> _roles = [];
    public IReadOnlySet<string> Roles => _roles;
    
    private readonly HashSet<string> _permissions = [];
    public IReadOnlySet<string> Permissions => _permissions;

    public UserContextMock SetIsAuthenticated(bool isAuthenticated)
    {
        IsAuthenticated = isAuthenticated;
        return this;
    }

    public UserContextMock SetUserId(Guid userId)
    {
        UserId = userId;
        return this;
    }

    public UserContextMock SetRoles(IEnumerable<string> roles)
    {
        _roles.UnionWith(roles);
        return this;
    }

    public UserContextMock SetPermissions(IEnumerable<string> permissions)
    {
        _permissions.UnionWith(permissions);
        return this;
    }
}