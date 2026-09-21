namespace Notification.Core.Interfaces;

public interface INotificationRenderer<in TNotification, out TContent>
{
	TContent Render(TNotification notification);
}
