using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Auth;

public class InvalidTokenException : BadRequestException
{
    [ExampleExceptionValues(false, "EXAMPLE_TOKEN")]
    public InvalidTokenException(string details) : base("Wrong token.", details)
    {
    }
}