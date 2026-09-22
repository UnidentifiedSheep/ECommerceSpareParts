namespace Notification.Core.Interfaces;

public interface INotificationSerializer
{
	string Serialize(INotification notification);
}
