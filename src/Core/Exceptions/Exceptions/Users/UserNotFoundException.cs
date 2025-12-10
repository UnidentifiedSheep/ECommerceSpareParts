using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Users;

public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException() : base("User not found")
    {
    }

    [ExampleExceptionValues(false,"0000-0000-0000-0000")]
    public UserNotFoundException(Guid id) : base("Не удалось найти пользователя", new { Id = id })
    {
    }

    [ExampleExceptionValues(true,"0000-0000-0000-0000", "0000-0000-0000-0001")]
    public UserNotFoundException(IEnumerable<Guid> ids) : base("Не удалось найти пользователя", new { Ids = ids })
    {
    }
}