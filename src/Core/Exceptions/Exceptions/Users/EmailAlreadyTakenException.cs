using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Users;

public class EmailAlreadyTakenException : ConflictException
{
    [ExampleExceptionValues(false,"example@some.com")]
    public EmailAlreadyTakenException(string? email) : base(
        "данная почта уже привязана к другому пользователю", new { Email = email })
    {
    }

    [ExampleExceptionValues(true,"example@some.com", "other-example@some.com")]
    public EmailAlreadyTakenException(IEnumerable<string> emails) : base(
        "Данные почты уже заняты другим пользователем/ями", new { Emails = emails })
    {
    }
}