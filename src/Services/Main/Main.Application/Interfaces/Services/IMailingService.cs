using Abstractions.Interfaces.Mail;
using Mailing.Core;

namespace Main.Application.Interfaces.Services;

public interface IMailingService
{
    Task QueueEmailAsync(
        IEmailData email,
        CancellationToken ct = default);

    Task QueueEmailAsync(
        IEnumerable<IEmailData> emails,
        CancellationToken ct = default);
    
    Task QueueEmailAsync(
        IEmailMessage email,
        CancellationToken ct = default);

    Task QueueEmailAsync(
        IEnumerable<IEmailMessage> emails,
        CancellationToken ct = default);
}
