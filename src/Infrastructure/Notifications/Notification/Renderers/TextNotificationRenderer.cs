using System.Diagnostics.CodeAnalysis;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;
using Notification.Interfaces;
using Notification.NotificationContents;

namespace Notification.Renderers;

public class TextNotificationRenderer(
	ILocalizer localizer,
	IContextualLocalizer contextualLocalizer
	) : INotificationRenderer<ISimpleNotification<ILocalizableMessage>, TextNotificationContent>
{
	public Task<TextNotificationContent> TryRenderAsync(ISimpleNotification<ILocalizableMessage> notification)
		=> Task.FromResult(new TextNotificationContent(notification.SelectedCulture == null
			? contextualLocalizer.Get(notification.Model)
			: localizer.Get(notification.Model, notification.SelectedCulture)));

	public Task<bool> TryRenderAsync(
		ISimpleNotification<ILocalizableMessage> notification,
		CancellationToken cancellationToken,
		[NotNullWhen(true)]
		out TextNotificationContent? content)
	{
		content = null;
		if (notification.SelectedCulture == null)
		{
			if (!contextualLocalizer.TryGet(notification.Model, out var value))
				return Task.FromResult(false);

			content = new TextNotificationContent(value);
			return Task.FromResult(true);
		}

		if (!localizer.TryGet(notification.Model, notification.SelectedCulture, out var v))
			return Task.FromResult(false);


		content = new TextNotificationContent(v);
		return Task.FromResult(true);
	}

	public Task<IReadOnlyList<TextNotificationContent?>> TryRenderAsync(
		IEnumerable<ISimpleNotification<ILocalizableMessage>> notifications,
		CancellationToken cancellationToken)
		=> Task.FromResult<IReadOnlyList<TextNotificationContent?>>(notifications
			.Select(x =>
			{
				TryRenderAsync(x, cancellationToken, out var content);
				return content;
			})
			.ToList());
}
