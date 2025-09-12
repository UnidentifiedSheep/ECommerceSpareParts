using Exceptions.Base;

namespace Exceptions.Exceptions.Users;

public class UserNameAlreadyTakenException(string userName) : BadRequestException($"Логин уже занят", new { UserName = userName })
{
    
}