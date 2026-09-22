namespace Notification.Core.Interfaces;

public interface INotificationService
{
	Task SendAsync(
		NotificationItem notification,
		CancellationToken cancellationToken = default);

	Task SendAsync(
		IReadOnlyCollection<NotificationItem> notifications,
		CancellationToken cancellationToken = default);
}
