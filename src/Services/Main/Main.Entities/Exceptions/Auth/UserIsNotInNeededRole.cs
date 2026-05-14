using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;
using Main.Enums;

namespace Main.Entities.Exceptions.Auth;

public class UserIsNotInNeededRole : BadRequestException, ILocalizableException
{
    public UserIsNotInNeededRole(Role role) : base(null, new { Role = role.ToString() })
    {
        Arguments = [role.ToString()];
    }

    public string MessageKey => "user.is.not.in.needed.role";
    public object[]? Arguments { get; }
}
