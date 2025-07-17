using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Users;

public class UserNameAlreadyTakenException(string userName) : BadRequestException($"Логин уже занят", new { UserName = userName })
{
    
}