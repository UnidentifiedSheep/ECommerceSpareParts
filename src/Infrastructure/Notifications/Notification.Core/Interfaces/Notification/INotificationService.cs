namespace Notification.Core.Interfaces.Notification;

public interface INotificationService
{
	Task QueueAsync(
		NotificationItem notification,
		CancellationToken cancellationToken = default);

	Task QueueAsync(
		IReadOnlyCollection<NotificationItem> notifications,
		CancellationToken cancellationToken = default);
}
