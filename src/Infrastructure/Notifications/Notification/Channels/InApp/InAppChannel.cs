using Abstractions.Interfaces.Persistence;
using Locan.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Notification.Core;
using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Recipients;
using Notification.Interfaces;
using Notification.NotificationContents;

namespace Notification.Channels.InApp;

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
		var rendered = await renderer.TryRenderAsync(
			notifications.Select(x => x.Notification),
			cancellationToken);

		return rendered
			.Select(x => x is not null
				? NotificationSendResult.Success()
				: NotificationSendResult.Failure("Unable to render notification."))
			.ToList();
	}
}
