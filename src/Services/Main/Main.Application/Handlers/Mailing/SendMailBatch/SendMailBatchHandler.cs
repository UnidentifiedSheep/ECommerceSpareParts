using Abstractions.Interfaces.Mail;
using Abstractions.Models.Mail;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Mailing;
using MediatR;

namespace Main.Application.Handlers.Mailing.SendMailBatch;

[Diagnostics]
[Transactional, AutoSave]
public record SendMailBatchCommand(int Batch = 100) : ICommand;

public class SendMailBatchHandler(
    IRepository<EmailOutBox, Guid> repository,
    IEmailSender sender) : ICommandHandler<SendMailBatchCommand>
{
    public async Task<Unit> Handle(
        SendMailBatchCommand request, 
        CancellationToken cancellationToken)
    {
        var batch = await repository.ListAsync(
            Criteria<EmailOutBox>
                .New()
                .ForUpdate(true, true)
                .Track()
                .Size(request.Batch)
                .Build(),
            cancellationToken);

        if (batch.Count == 0) return Unit.Value;

        var messages = new List<EmailMessage>();
        
        batch.ForEach(m =>
        {
            messages.Add(new EmailMessage(m.Subject, m.To, m.Body));
            m.Sent();
        });

        await sender.SendBatchAsync(messages, cancellationToken);
        
        return Unit.Value;
    }
}