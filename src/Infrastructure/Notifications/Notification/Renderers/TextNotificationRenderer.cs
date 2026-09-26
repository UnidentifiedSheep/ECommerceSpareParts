using Notification.Core.Interfaces.Notification;
using Notification.Interfaces;

namespace Notification.Renderers;

public class TextNotificationRenderer : INotificationRenderer<ITextNotification>
{
	public Task<INotificationContent?> TryRenderAsync(
		ITextNotification notification,
		CancellationToken cancellationToken)
		=> Task.FromResult<INotificationContent?>(new NotificationContent(notification.GetModel().AsText));

	public Task<IReadOnlyList<INotificationContent?>> TryRenderAsync(
		IEnumerable<ITextNotification> notifications,
		CancellationToken cancellationToken)
		=> Task.FromResult<IReadOnlyList<INotificationContent?>>(notifications
			.Select(INotificationContent? (notification) => new NotificationContent(notification.GetModel().AsText))
			.ToList());
}
