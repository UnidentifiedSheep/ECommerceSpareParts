using Microsoft.Extensions.Logging;
using Notification.Core;
using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Recipients;
using Notification.Interfaces;

namespace Notification.Channels.Email;

public class EmailChannel(
	INotificationRenderer<INotification> renderer,
	IEmailSender sender,
	IEnumerable<IChannelDeliveryObserver<EmailReceipt, EmailRecipient>> observers,
	ILogger<EmailChannel> logger
	) : NotificationChannelBase<INotification, EmailRecipient, EmailReceipt>(observers, logger)
{
	public override string SystemName => EmailRecipient.ChannelName;

	public override async Task<IReadOnlyList<SendResult>> SendBatchAsync(
		IReadOnlyCollection<NotificationDelivery<INotification, EmailRecipient>> notifications,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(notifications);
		if (notifications.Count == 0) return [];

		var deliveries = notifications.ToArray();
		var rendered = await renderer.TryRenderAsync(
			deliveries.Select(x => x.Notification),
			cancellationToken);
		if (rendered.Count != deliveries.Length)
			throw new InvalidOperationException(
				$"Channel '{SystemName}' received {rendered.Count} rendered contents for {deliveries.Length} deliveries.");

		var results = new SendResult[deliveries.Length];
		var messages = new List<EmailMessage>(deliveries.Length);
		var messageIndexes = new List<int>(deliveries.Length);

		for (var i = 0; i < deliveries.Length; i++)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var content = rendered[i];
			if (content is null || string.IsNullOrWhiteSpace(content.Text))
			{
				results[i] = SendResult.Failure("Unable to render notification.");
				continue;
			}

			var recipient = deliveries[i].Recipient;
			messages.Add(new EmailMessage(recipient.Subject, recipient.Email, content.Text));
			messageIndexes.Add(i);
		}

		if (messages.Count == 0) return results;

		var sent = await sender.SendBatchAsync(messages, cancellationToken);
		if (sent.Count != messages.Count)
			throw new InvalidOperationException(
				$"Channel '{SystemName}' received {sent.Count} send results for {messages.Count} messages.");

		var receipts = new List<EmailReceipt>();
		for (var i = 0; i < sent.Count; i++)
		{
			var deliveryIndex = messageIndexes[i];
			results[deliveryIndex] = sent[i];
			if (sent[i].Succeeded)
				receipts.Add(new EmailReceipt(deliveries[deliveryIndex].Recipient));
		}

		await NotifyObserversAsync(receipts, cancellationToken);
		return results;
	}
}
