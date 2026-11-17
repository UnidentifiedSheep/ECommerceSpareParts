using Notification.Core.Interfaces.Notification;

namespace Main.Modules.Notifications.Application.Delivery.Interfaces;

public interface INotificationRenderer<in TNotification>
{
	Task<INotificationContent?> TryRenderAsync(
		TNotification notification,
		CancellationToken cancellationToken);

	Task<IReadOnlyList<INotificationContent?>> TryRenderAsync(
		IEnumerable<TNotification> notifications,
		CancellationToken cancellationToken);
}
