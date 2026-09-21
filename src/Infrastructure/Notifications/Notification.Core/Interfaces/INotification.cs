using System.Globalization;
using Locan.Core.Interfaces;

namespace Notification.Core.Interfaces;

public interface ISimpleNotification<out TModel>
	: INotification<TModel> where TModel : ILocalizableMessage;

public interface INotification<out TModel> : INotification
{
	TModel Model { get; }
}

public interface INotification
{
	CultureInfo? SelectedCulture { get; }
}
