using NamedObject.Core.Interfaces;
using Notification.Core.Interfaces.Recipient;

namespace Notification.Core.Interfaces.Notification;

public interface INotificationChannel<TNotification, TDestination> : INotificationChannel
	where TNotification : INotification
	where TDestination : INotificationRecipient
{
	Task<SendResult> SendAsync(
		NotificationDelivery<TNotification, TDestination> notification,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<SendResult>> SendBatchAsync(
		IReadOnlyCollection<NotificationDelivery<TNotification, TDestination>> notifications,
		CancellationToken cancellationToken = default);
}

public interface INotificationChannel : INamedObject
{
	bool CanHandle(
		INotification notification,
		INotificationRecipient recipient);

	Task<SendResult> SendAsync(
		NotificationDelivery notification,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<SendResult>> SendBatchAsync(
		IReadOnlyCollection<NotificationDelivery> notifications,
		CancellationToken cancellationToken = default);
}
