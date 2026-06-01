using Abstractions.Interfaces.Mail;

namespace Abstractions.Models.Mail;

public class EmailMessage(
    string subject,
    string to,
    string body) : IEmailMessage
{
    public string Subject => subject;
    public string To => to;
    public string GetHtmlBody() => body;
}