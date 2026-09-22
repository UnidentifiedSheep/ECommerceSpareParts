using System.Text.Json;
using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;

namespace Notification;

public sealed class NotificationSerializer : INotificationSerializer
{
	public string Serialize(INotification notification)
	{
		ArgumentNullException.ThrowIfNull(notification);

		var model = notification.GetModel();
		return JsonSerializer.Serialize(model, model.GetType());
	}

	public TModel Deserialize<TModel>(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		return JsonSerializer.Deserialize<TModel>(json)
			?? throw new InvalidOperationException(
				$"Notification model '{typeof(TModel).Name}' is empty.");
	}
}
