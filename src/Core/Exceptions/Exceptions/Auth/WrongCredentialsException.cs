using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Auth;

public class WrongCredentialsException : BadRequestException
{
    [ExampleExceptionValues(false, "EXAMPLE_DETAILS")]
    public WrongCredentialsException(string details) : base("Неверный логин или пароль", details)
    {
    }
}