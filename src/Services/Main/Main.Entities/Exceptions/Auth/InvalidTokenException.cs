using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Auth;

public class InvalidTokenException : BadRequestException, ILocalizableException
{
    public InvalidTokenException(string token) : base(null, new { Token = token })
    {
    }

    public string MessageKey => "invalid.token";
    public object[]? Arguments => null;
}