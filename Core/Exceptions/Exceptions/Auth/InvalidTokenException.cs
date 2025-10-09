using Exceptions.Base;

namespace Exceptions.Exceptions.Auth;

public class InvalidTokenException : BadRequestException
{
    public InvalidTokenException(string details) : base("Wrong token.", details)
    {
    }
}