using System.Globalization;
using Locan.Core.Interfaces;
using NamedObject.Core.Interfaces;

namespace Notification.Core.Interfaces.Notification;

public interface ISimpleNotification<out TModel>
	: INotification<TModel> where TModel : ILocalizableMessage;

public interface INotification<out TModel> : INotification
{
	TModel Model { get; }

	object INotification.GetModel() => Model!;
}

public interface INotification : INamedObject
{
	CultureInfo? SelectedCulture { get; }

	object GetModel();
}
