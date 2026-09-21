using System.Diagnostics.CodeAnalysis;
using Notification.Core.Interfaces;

namespace Notification.Interfaces;

public interface INotificationRenderer<in TNotification, TContent>
	where TContent : INotificationContent
{
	Task<bool> TryRenderAsync(
		TNotification notification,
		CancellationToken cancellationToken,
		[NotNullWhen(true)]
		out TContent? content);

	Task<IReadOnlyList<TContent?>> TryRenderAsync(
		IEnumerable<TNotification> notifications,
		CancellationToken cancellationToken);
}
