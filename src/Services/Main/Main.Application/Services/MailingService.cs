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
        var model = EmailOutBox.Create(
            email.Subject,
            email.To,
            email.GetHtmlBody());

        await unitOfWork.AddAsync(model, ct);
    }
}