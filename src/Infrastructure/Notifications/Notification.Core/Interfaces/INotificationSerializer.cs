namespace Notification.Core.Interfaces;

public interface INotificationSerializer
{
	string Serialize(INotification notification);
	TModel Deserialize<TModel>(string json);
}
