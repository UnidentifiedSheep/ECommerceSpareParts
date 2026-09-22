namespace Notification.Core.Interfaces.Notification;

public interface INotificationService
{
	Task SendAsync(
		NotificationItem notification,
		CancellationToken cancellationToken = default);

	Task SendAsync(
		IReadOnlyCollection<NotificationItem> notifications,
		CancellationToken cancellationToken = default);
}
