using Notification.Core.Interfaces.Recipient;

namespace Notification.Core.Interfaces.Notification;

public interface INotificationService
{
	Task<SendResult> SendAsync(
		INotificationRecipient recipient,
		INotification notification,
		CancellationToken cancellationToken = default);

	Task QueueAsync(
		NotificationItem notification,
		CancellationToken cancellationToken = default);

	Task QueueAsync(
		IReadOnlyCollection<NotificationItem> notifications,
		CancellationToken cancellationToken = default);
}
