using Main.Modules.Notifications.Application.Delivery.Interfaces;
using Notification.Core.Interfaces.Notification;
using RazorLight;

namespace Main.Modules.Notifications.Application.Delivery.Renderers;

public class HtmlNotificationRenderer(
	IRazorLightEngine engine) : INotificationRenderer<INotification>
{
	public async Task<INotificationContent?> TryRenderAsync(
		INotification notification,
		CancellationToken cancellationToken)
	{
		var model = notification.GetModel();
		var body = await engine.CompileRenderAsync(
			$"{notification.SystemName}.cshtml",
			model);

		return string.IsNullOrWhiteSpace(body)
			? null
			: new NotificationContent(
				body,
				model is INotificationModelWithTitle titled ? titled.Title : string.Empty);
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
