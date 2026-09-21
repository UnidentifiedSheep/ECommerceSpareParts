using Locan.Core.Interfaces;
using Notification.Core;
using Notification.Core.Interfaces;
using Notification.Core.Recipients;
using Notification.Interfaces;
using Notification.NotificationContents;

namespace Notification.Channels;

public class InAppChannel(
	INotificationRenderer<ISimpleNotification<ILocalizableMessage>, TextNotificationContent> renderer
	) : NotificationChannelBase<ISimpleNotification<ILocalizableMessage>, InAppRecipient>
{
	public override string SystemName => "InApp";
	public override async Task<IReadOnlyList<bool>> SendBatchAsync(
		IEnumerable<NotificationDelivery<ISimpleNotification<ILocalizableMessage>, InAppRecipient>> notifications,
		CancellationToken cancellationToken = default)
	{
		var rendered = await renderer.TryRenderAsync(
			notifications.Select(x => x.Notification),
			cancellationToken);

		return rendered.Select(x => x != null).ToList();
	}
}
