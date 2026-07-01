using Abstractions.Interfaces.Mail;
using Abstractions.Models.Mail;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Mailing;
using Main.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Main.Application.Handlers.Mailing.SendMailBatch;

[Diagnostics]
[Transactional]
[AutoSave]
public record SendMailBatchCommand(int Batch = 100) : ICommand;

public class SendMailBatchHandler(
    IRepository<EmailOutBox, Guid> repository,
    ILogger<SendMailBatchCommand> logger,
    IEmailSender sender
) : ICommandHandler<SendMailBatchCommand>
{
    public async Task<Unit> Handle(
        SendMailBatchCommand request,
        CancellationToken cancellationToken)
    {
        var batch = await repository.ListAsync(
            Criteria<EmailOutBox>
                .New()
                .OrderByDesc(x => x.Id)
                .Where(x => x.Status == EmailStatus.Pending)
                .ForUpdate(true, true)
                .Track()
                .Size(request.Batch)
                .Build(),
            cancellationToken);

        if (batch.Count == 0) return Unit.Value;

        var messages = new List<EmailMessage>();

        batch.ForEach(m =>
        {
            messages.Add(
                new EmailMessage(
                    m.Subject,
                    m.To,
                    m.Body));
            m.Sent();
        });

        logger.LogInformation("Sending mails. Count: {Count}", messages.Count);

        await sender.SendBatchAsync(messages, cancellationToken);

        return Unit.Value;
    }
}