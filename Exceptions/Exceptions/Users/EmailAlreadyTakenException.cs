using Exceptions.Base;

namespace Exceptions.Exceptions.Users;

public class EmailAlreadyTakenException(string? email)
    : BadRequestException($"'{email}' данная почта уже привязана к другому пользователю")
{
}