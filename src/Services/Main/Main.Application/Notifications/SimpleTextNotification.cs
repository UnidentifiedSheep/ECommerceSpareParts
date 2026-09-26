using Notification.Core.Interfaces.Notification;

namespace Main.Application.Notifications;

public class SimpleTextNotification(string text) : ITextNotification
{
	public string SystemName => "CustomTextNotification";
	public INotificationModel GetModel() => new SimpleTextNotificationData(text);
}

public class SimpleTextNotificationData(string text) : INotificationModel
{
	public string AsText => text;
}
