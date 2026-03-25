using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Auth;

public class WrongCredentialsException : BadRequestException, ILocalizableException
{
    public string MessageKey => "wrong.credentials";
    public object[]? Arguments => null;
    public WrongCredentialsException(string? email, string? password) 
        : base(null, new  { Email = email, Password = password }) { }
}