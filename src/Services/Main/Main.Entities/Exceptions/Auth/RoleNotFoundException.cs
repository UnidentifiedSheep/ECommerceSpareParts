using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Auth;

public class RoleNotFoundException : NotFoundException, ILocalizableException
{
    public RoleNotFoundException(Guid id) : base(null, new { Id = id })
    {
        MessageKey = "role.not.found";
        Arguments = null;
    }

    public RoleNotFoundException(string roleName) : base(null, new { Name = roleName })
    {
        MessageKey = "role.not.found.with.role.name";
        Arguments = [roleName];
    }

    public string MessageKey { get; }
    public object[]? Arguments { get; }
}