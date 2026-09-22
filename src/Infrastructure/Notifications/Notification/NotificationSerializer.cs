using System.Text.Json;
using Notification.Core.Interfaces;

namespace Notification;

public sealed class NotificationSerializer : INotificationSerializer
{
	public string Serialize(INotification notification)
	{
		ArgumentNullException.ThrowIfNull(notification);

		var model = notification.GetModel();
		return JsonSerializer.Serialize(model, model.GetType());
	}
}
