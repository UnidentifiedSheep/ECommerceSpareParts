using Notification.Core.Interfaces.Notification;
using Notification.Interfaces;
using RazorLight;

namespace Notification.Renderers;

public class HtmlNotificationRenderer(
	IRazorLightEngine engine) : INotificationRenderer<INotification>
{
	public async Task<INotificationContent?> TryRenderAsync(
		INotification notification,
		CancellationToken cancellationToken)
	{
		var body = await engine.CompileRenderAsync(
			key: $"{notification.SystemName}.cshtml",
			model: notification.GetModel());

		return string.IsNullOrWhiteSpace(body) ? null : new NotificationContent(body);
	}

	public async Task<IReadOnlyList<INotificationContent?>> TryRenderAsync(
		IEnumerable<INotification> notifications,
		CancellationToken cancellationToken)
	{
		var result = new List<INotificationContent?>();
		foreach (var notification in notifications)
			result.Add(await TryRenderAsync(notification, cancellationToken));
		return result;
	}
}
