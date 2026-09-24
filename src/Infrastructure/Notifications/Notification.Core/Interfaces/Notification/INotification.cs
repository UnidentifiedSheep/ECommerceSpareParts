using NamedObject.Core.Interfaces;

namespace Notification.Core.Interfaces.Notification;

public interface ISimpleNotification : INotification<string>;

public interface INotification<out TModel> : INotification
{
	TModel Model { get; }

	object INotification.GetModel() => Model!;
}

public interface INotification : INamedObject
{
	object GetModel();
}
