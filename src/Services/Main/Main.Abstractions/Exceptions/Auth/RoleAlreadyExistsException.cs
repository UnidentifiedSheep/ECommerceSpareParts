using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Auth;

public class RoleAlreadyExistsException : BadRequestException, ILocalizableException
{
    public string MessageKey => "role.already.exists";
    public object[]? Arguments { get; }
    public RoleAlreadyExistsException(string roleName) : base(null, new { Name = roleName })
    {
        Arguments = [roleName];
    }
}