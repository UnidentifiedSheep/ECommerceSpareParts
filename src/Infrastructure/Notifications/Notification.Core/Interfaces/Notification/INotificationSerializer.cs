namespace Notification.Core.Interfaces.Notification;

public interface INotificationSerializer
{
	string Serialize(INotification notification);
	TModel Deserialize<TModel>(string json);
}
