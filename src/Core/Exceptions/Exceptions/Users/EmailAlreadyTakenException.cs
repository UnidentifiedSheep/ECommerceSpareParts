using Exceptions.Base;

namespace Exceptions.Exceptions.Users;

public class EmailAlreadyTakenException : BadRequestException
{
    public EmailAlreadyTakenException(string? email) : base(
        $"'{email}' данная почта уже привязана к другому пользователю")
    {
    }

    public EmailAlreadyTakenException(IEnumerable<string> emails) : base(
        "Данные почты уже заняты другим пользователем/ями", new { Emails = emails })
    {
    }
}