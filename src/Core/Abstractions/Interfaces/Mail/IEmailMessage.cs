namespace Abstractions.Interfaces.Mail;

public interface IEmailMessage
{
    string Subject { get; }
    string To { get; }

    string GetHtmlBody();
}