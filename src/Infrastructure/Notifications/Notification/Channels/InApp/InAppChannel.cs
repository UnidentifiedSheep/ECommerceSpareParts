using Locan.Core.Interfaces;
using Notification.Core;
using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Recipients;
using Notification.Interfaces;
using Notification.NotificationContents;

namespace Notification.Channels.InApp;

public class InAppChannel(
	INotificationRenderer<ISimpleNotification<ILocalizableMessage>, TextNotificationContent> renderer,
	IEnumerable<IChannelDeliveryObserver<InAppReceipt, InAppRecipient>> observers
	) : NotificationChannelBase<ISimpleNotification<ILocalizableMessage>, InAppRecipient, InAppReceipt>(observers)
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
