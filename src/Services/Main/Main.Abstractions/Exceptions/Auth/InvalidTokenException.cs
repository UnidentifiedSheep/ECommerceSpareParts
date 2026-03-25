using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Auth;

public class InvalidTokenException : BadRequestException, ILocalizableException
{
    public string MessageKey => "invalid.token";
    public object[]? Arguments => null;
    public InvalidTokenException(string token) : base(null, new { Token = token })
    {
    }
}