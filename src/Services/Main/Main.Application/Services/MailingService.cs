using Abstractions.Interfaces.Mail;
using Abstractions.Interfaces.Persistence;
using Mailing.Core;
using Main.Application.Interfaces.Services;
using Main.Entities.Mailing;

namespace Main.Application.Services;

public class MailingService(IUnitOfWork unitOfWork, IEmailMessageRenderer renderer) : IMailingService
{
	public Task QueueEmailAsync(IEmailData email, CancellationToken ct = default) =>
		QueueEmailAsync([email], ct);
	public async Task QueueEmailAsync(IEnumerable<IEmailData> emails, CancellationToken ct = default)
	{
		var rendered = new List<IEmailMessage>();
		foreach (var email in emails)
			rendered.Add(await renderer.RenderAsync(email, ct));
		await QueueEmailAsync(rendered, ct);
	}

	public async Task QueueEmailAsync(IEmailMessage email, CancellationToken ct = default) =>
		await QueueEmailAsync([email], ct);

	public async Task QueueEmailAsync(IEnumerable<IEmailMessage> emails, CancellationToken ct = default)
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
