using Notification.Core.Interfaces.Notification;
using Notification.Interfaces;

namespace Notification.Renderers;

public class TextNotificationRenderer : INotificationRenderer<ISimpleNotification>
{
	public Task<INotificationContent?> TryRenderAsync(
		ISimpleNotification notification,
		CancellationToken cancellationToken)
		=> Task.FromResult<INotificationContent?>(new NotificationContent(notification.Model));

	public Task<IReadOnlyList<INotificationContent?>> TryRenderAsync(
		IEnumerable<ISimpleNotification> notifications,
		CancellationToken cancellationToken)
		=> Task.FromResult<IReadOnlyList<INotificationContent?>>(notifications
			.Select(INotificationContent? (notification) => new NotificationContent(notification.Model))
			.ToList());
}
