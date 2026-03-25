using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Auth;

public class UserNotFoundException : NotFoundException, ILocalizableException
{
    public string MessageKey => "user.not.found";
    public object[]? Arguments => null;
    public UserNotFoundException(Guid id) : base(null, new { Id = id })
    {
    }
}