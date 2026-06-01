using Abstractions.Interfaces.Mail;

namespace Main.Application.Interfaces.Services;

public interface IMailingService
{
    Task QueueToOutbox(
        IEmailMessage email,
        CancellationToken ct = default);
}