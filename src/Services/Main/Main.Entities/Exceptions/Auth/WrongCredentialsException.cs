using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Auth;

public class WrongCredentialsException : BadRequestException, ILocalizableException
{
    public WrongCredentialsException(string? email, string? password)
        : base(null, new { Email = email, Password = password })
    {
    }

    public string MessageKey => "wrong.credentials";
    public object[]? Arguments => null;
}