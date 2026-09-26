using NamedObject.Core.Interfaces;

namespace Notification.Core.Interfaces.Notification;

public interface INotificationDefinition : INamedObject
{
	INotification FromEntity(Entities.Notification entity);

	Entities.Notification ToEntity(
		Guid userId,
		INotification notification);
}

public interface INotificationDefinition<TNotification> : INotificationDefinition
	where TNotification : INotification
{
	new TNotification FromEntity(Entities.Notification entity);

	Entities.Notification ToEntity(
		Guid userId,
		TNotification notification);
}
