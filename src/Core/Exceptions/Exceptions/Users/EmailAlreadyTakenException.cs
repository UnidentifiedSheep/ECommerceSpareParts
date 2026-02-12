using Exceptions.Base;

namespace Exceptions.Exceptions.Users;

public class EmailAlreadyTakenException : ConflictException
{
    public EmailAlreadyTakenException(string? email) : base(
        "данная почта уже привязана к другому пользователю", new { Email = email })
    {
    }

    public EmailAlreadyTakenException(IEnumerable<string> emails) : base(
        "Данные почты уже заняты другим пользователем/ями", new { Emails = emails })
    {
    }
}