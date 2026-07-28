using Abstractions.Interfaces.Mail;

namespace Main.Application.Interfaces.Services;

public interface IMailingService
{
    Task QueueToOutbox(
        IEmailMessage email,
        CancellationToken ct = default);

    Task QueueToOutbox(
        IEnumerable<IEmailMessage> emails,
        CancellationToken ct = default);
}
