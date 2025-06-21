using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Users;

public class UserNameAlreadyTakenException(string userName) : BadRequestException($"Логин '{userName}' уже занят")
{
    
}