using NamedObject.Core.Interfaces;

namespace Notification.Core.Interfaces;

public interface INotificationChannel<TNotification, TDestination>
	: INamedObject
	where TNotification : INotification
	where TDestination : INotificationRecipient
{
	Task<bool> SendAsync(
		NotificationDelivery<TNotification, TDestination> notification,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<bool>> SendBatchAsync(
		IEnumerable<NotificationDelivery<TNotification, TDestination>> notifications,
		CancellationToken cancellationToken = default);
}
