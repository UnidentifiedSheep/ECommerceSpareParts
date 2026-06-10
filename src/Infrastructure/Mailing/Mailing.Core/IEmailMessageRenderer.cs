using Abstractions.Interfaces.Mail;

namespace Mailing.Core;

public interface IEmailMessageRenderer
{
    Task<IEmailMessage> RenderAsync<TTemplate>(
        TTemplate templateData,
        CancellationToken cancellationToken = default)
        where TTemplate : IEmailData;
}