using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Auth;

public class RoleAlreadyExistsException : BadRequestException, ILocalizableException
{
    public RoleAlreadyExistsException(string roleName) : base(null, new { Name = roleName })
    {
        Arguments = [roleName];
    }

    public string MessageKey => "role.already.exists";
    public object[]? Arguments { get; }
}