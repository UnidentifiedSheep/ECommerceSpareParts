using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Users;

public class UserNameAlreadyTakenException(string userName) : BadRequestException($"Логин уже занят", new { UserName = userName })
{
    
}