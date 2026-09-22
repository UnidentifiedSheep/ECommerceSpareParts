using Abstractions.Interfaces.Persistence;
using Notification.Core.Interfaces;
using NotificationEntity = Notification.Core.Entities.Notification;

namespace Notification.Persistence;

public sealed class NotificationStore(IUnitOfWork unitOfWork) : INotificationStore
{
	public async Task AddRangeAsync(
		IReadOnlyCollection<NotificationEntity> notifications,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(notifications);

		if (notifications.Count == 0) return;

		await unitOfWork.AddRangeAsync(notifications, cancellationToken);
	}
}
