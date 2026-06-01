using Abstractions.Interfaces.Mail;

namespace Mail.Interface;

public interface IEmailSender
{
    Task SendAsync(
        IEmailMessage message,
        CancellationToken token = default);
}