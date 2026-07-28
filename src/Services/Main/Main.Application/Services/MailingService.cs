using Abstractions.Interfaces.Mail;
using Abstractions.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities.Mailing;

namespace Main.Application.Services;

public class MailingService(
    IUnitOfWork unitOfWork
) : IMailingService
{
    public async Task QueueToOutbox(
        IEmailMessage email,
        CancellationToken ct = default)
    {
        await QueueToOutbox([email], ct);
    }

    public async Task QueueToOutbox(
        IEnumerable<IEmailMessage> emails,
        CancellationToken ct = default)
    {
        var models = emails
            .Select(email => EmailOutBox.Create(
                email.Subject,
                email.To,
                email.GetHtmlBody()))
            .ToList();

        if (models.Count > 0)
            await unitOfWork.AddRangeAsync(models, ct);
    }
}
