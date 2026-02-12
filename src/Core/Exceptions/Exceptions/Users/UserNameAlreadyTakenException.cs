using Exceptions.Base;

namespace Exceptions.Exceptions.Users;

public class UserNameAlreadyTakenException : BadRequestException
{
    public UserNameAlreadyTakenException(string userName) : base("Логин уже занят", new { UserName = userName })
    {
    }
}