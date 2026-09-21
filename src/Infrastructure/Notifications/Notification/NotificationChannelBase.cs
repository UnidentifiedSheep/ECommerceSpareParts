using Notification.Core;
using Notification.Core.Interfaces;

namespace Notification;

public abstract class NotificationChannelBase<TNotification, TDestination> : INotificationChannel<TNotification, TDestination>
	where TNotification : INotification
	where TDestination : INotificationRecipient
{
	public abstract string SystemName { get; }

	public virtual async Task<bool> SendAsync(
		NotificationDelivery<TNotification, TDestination> notification,
		CancellationToken cancellationToken = default)
		=> (await SendBatchAsync([notification], cancellationToken))[0];

	public abstract Task<IReadOnlyList<bool>> SendBatchAsync(
		IEnumerable<NotificationDelivery<TNotification, TDestination>> notifications,
		CancellationToken cancellationToken = default);
}
