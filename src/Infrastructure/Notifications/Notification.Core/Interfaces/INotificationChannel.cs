using NamedObject.Core.Interfaces;

namespace Notification.Core.Interfaces;

public interface INotificationChannel<TNotification, TDestination> : INotificationChannel
	where TNotification : INotification
	where TDestination : INotificationRecipient
{
	Task<NotificationSendResult> SendAsync(
		NotificationDelivery<TNotification, TDestination> notification,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<NotificationSendResult>> SendBatchAsync(
		IReadOnlyCollection<NotificationDelivery<TNotification, TDestination>> notifications,
		CancellationToken cancellationToken = default);
}

public interface INotificationChannel : INamedObject
{
	bool CanHandle(
		INotification notification,
		INotificationRecipient recipient);

	Task<NotificationSendResult> SendAsync(
		NotificationDelivery notification,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<NotificationSendResult>> SendBatchAsync(
		IReadOnlyCollection<NotificationDelivery> notifications,
		CancellationToken cancellationToken = default);
}
