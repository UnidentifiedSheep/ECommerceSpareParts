using Abstractions.Interfaces.Persistence;
using Attributes;
using Locan.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Notification.Core;
using Notification.Core.Entities;
using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Recipients;
using Notification.Interfaces;
using Notification.NotificationContents;

namespace Notification.Channels;

public class InAppChannel(
	INotificationRenderer<ISimpleNotification<ILocalizableMessage>, TextNotificationContent> renderer,
	IEnumerable<IChannelDeliveryObserver<InAppReceipt, InAppRecipient>> observers,
	IUnitOfWork unitOfWork,
	ILogger<InAppChannel> logger
	) : NotificationChannelBase<ISimpleNotification<ILocalizableMessage>, InAppRecipient, InAppReceipt>(observers, logger)
{
	public override string SystemName => InAppRecipient.ChannelName;

	public override async Task<IReadOnlyList<NotificationSendResult>> SendBatchAsync(
		IReadOnlyCollection<NotificationDelivery<ISimpleNotification<ILocalizableMessage>, InAppRecipient>> notifications,
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

		var results = new NotificationSendResult[deliveries.Length];
		var rows = new List<InAppNotification>(deliveries.Length);
		var recipients = new List<InAppRecipient>(deliveries.Length);
		for (var i = 0; i < deliveries.Length; i++)
		{
			var content = rendered[i];
			if (content is null)
			{
				results[i] = NotificationSendResult.Failure("Unable to render notification.");
				continue;
			}

			var recipient = deliveries[i].Recipient;
			rows.Add(InAppNotification.Create(recipient.UserId, content.Text));
			recipients.Add(recipient);
			results[i] = NotificationSendResult.Success();
		}

		if (rows.Count == 0) return results;

		await unitOfWork.ExecuteWithTransaction(
			new TransactionalAttribute(),
			async () =>
			{
				await unitOfWork.AddRangeAsync(rows, cancellationToken);
				await unitOfWork.SaveChangesAsync(cancellationToken);

				var receipts = rows
					.Select((row, i) => new InAppReceipt(recipients[i], row.Id))
					.ToArray();
				await NotifyObserversAsync(receipts, cancellationToken);
			},
			cancellationToken);

		return results;
	}
}
