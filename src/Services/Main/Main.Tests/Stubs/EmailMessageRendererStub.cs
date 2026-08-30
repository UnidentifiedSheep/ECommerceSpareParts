using Abstractions.Interfaces.Mail;
using Abstractions.Models.Mail;
using Mailing.Core;

namespace Tests.Stubs;

public sealed class EmailMessageRendererStub : IEmailMessageRenderer
{
	public Task<IEmailMessage> RenderAsync<TTemplate>(
		TTemplate templateData,
		CancellationToken cancellationToken = default) where TTemplate : IEmailData
	{
		return Task.FromResult<IEmailMessage>(
			new EmailMessage(
				templateData.Subject,
				templateData.To,
				templateData.TemplateName));
	}
}
