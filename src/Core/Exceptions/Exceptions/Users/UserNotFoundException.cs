using Exceptions.Base;

namespace Exceptions.Exceptions.Users;

public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException() : base("User not found")
    {
    }

    public UserNotFoundException(Guid id) : base("Не удалось найти пользователя", new { Id = id })
    {
    }

    public UserNotFoundException(IEnumerable<Guid> ids) : base("Не удалось найти пользователя", new { Ids = ids })
    {
    }
}