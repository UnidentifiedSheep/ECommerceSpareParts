using NamedObject.Core.Interfaces;

namespace Notification.Core.Interfaces.Notification;

public interface ITextNotification : INotification;

public interface INotification<out TModel> : INotification
	where TModel : INotificationModel
{
	TModel Model { get; }

	INotificationModel INotification.GetModel() => Model;
}

public interface INotification : INamedObject
{
	INotificationModel GetModel();
}
