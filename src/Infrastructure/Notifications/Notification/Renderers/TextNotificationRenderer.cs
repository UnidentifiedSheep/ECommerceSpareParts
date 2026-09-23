using System.Diagnostics.CodeAnalysis;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Notification.Core.Interfaces.Notification;
using Notification.Interfaces;

namespace Notification.Renderers;

public class TextNotificationRenderer(
	ILocalizer localizer,
	IContextualLocalizer contextualLocalizer
	) : INotificationRenderer<ISimpleNotification<ILocalizableMessage>>
{
	public Task<INotificationContent?> TryRenderAsync(
		ISimpleNotification<ILocalizableMessage> notification,
		CancellationToken cancellationToken)
		=> Task.FromResult(TryRenderCore(notification));

	public Task<IReadOnlyList<INotificationContent?>> TryRenderAsync(
		IEnumerable<ISimpleNotification<ILocalizableMessage>> notifications,
		CancellationToken cancellationToken)
		=> Task.FromResult<IReadOnlyList<INotificationContent?>>(notifications
			.Select(TryRenderCore)
			.ToList());

	private INotificationContent? TryRenderCore(ISimpleNotification<ILocalizableMessage> notification)
	{
		if (notification.SelectedCulture == null)
			return !contextualLocalizer.TryGet(notification.Model, out var value)
				? null
				: new NotificationContent(value);

		return !localizer.TryGet(notification.Model, notification.SelectedCulture, out var v)
			? null
			: new NotificationContent(v);
	}
}
