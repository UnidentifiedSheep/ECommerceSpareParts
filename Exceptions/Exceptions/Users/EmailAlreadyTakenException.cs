using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Users;

public class EmailAlreadyTakenException(string? email) : BadRequestException($"'{email}' данная почта уже привязана к другому пользователю")
{
    
}