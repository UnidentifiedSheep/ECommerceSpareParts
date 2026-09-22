using NotificationEntity = Notification.Core.Entities.Notification;

namespace Notification.Core.Interfaces.Notification;

public interface INotificationStore
{
	Task AddRangeAsync(
		IReadOnlyCollection<NotificationEntity> notifications,
		CancellationToken cancellationToken = default);
}
