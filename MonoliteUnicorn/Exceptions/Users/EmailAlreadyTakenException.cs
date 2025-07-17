using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Users;

public class EmailAlreadyTakenException(string? email) : BadRequestException($"'{email}' данная почта уже привязана к другому пользователю")
{
    
}