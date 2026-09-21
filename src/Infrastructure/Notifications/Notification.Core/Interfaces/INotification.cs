using System.Globalization;
using Locan.Core.Interfaces;

namespace Notification.Core.Interfaces;

public interface ISimpleNotification<out TModel>
	: INotification<TModel> where TModel : ILocalizableMessage;

public interface IRichNotification<out TModel> : INotification<TModel>;

public interface INotification<out TModel>
{
	CultureInfo? SelectedCulture { get; }
	TModel Model { get; }
}
