using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Users;

public class UserNameAlreadyTakenException : BadRequestException
{
    [ExampleExceptionValues(false,"ExampleUserLogin")]
    public UserNameAlreadyTakenException(string userName) : base("Логин уже занят", new { UserName = userName })
    {
    }
}