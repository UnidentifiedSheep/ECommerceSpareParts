namespace Abstractions.Interfaces.Mail;

public interface IEmailSender
{
    Task SendAsync(
        IEmailMessage message,
        CancellationToken token = default);

    Task SendBatchAsync(
        IEnumerable<IEmailMessage> messages,
        CancellationToken token = default);
}