using NamedObject.Core.Interfaces;

namespace Notification.Core.Interfaces.Notification;

public interface INotificationDefinition : INamedObject
{
	INotification FromEntity(Main.Entities.Notification.Notification entity);

	Main.Entities.Notification.Notification ToEntity(
		Guid userId,
		INotification notification);
}

public interface INotificationDefinition<TNotification> : INotificationDefinition
	where TNotification : INotification
{
	new TNotification FromEntity(Main.Entities.Notification.Notification entity);

	Main.Entities.Notification.Notification ToEntity(
		Guid userId,
		TNotification notification);
}
