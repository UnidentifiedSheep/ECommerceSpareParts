using Microsoft.Extensions.DependencyInjection;
using NamedObject;
using Notification.Core;
using Notification.Core.Interfaces.Notification;

namespace Notification.Extensions;

public static class NotificationRegistrationExtensions
{
	public static IServiceCollection AddNotification<TNotification, TModel>(
		this IServiceCollection services,
		string systemName,
		Func<TModel, TNotification> createNotification)
		where TNotification : INotification<TModel>
		where TModel : INotificationModel
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(systemName);
		ArgumentNullException.ThrowIfNull(createNotification);

		services.AddNamedObjectRegistry();
		services.AddSingleton<INotificationDefinition>(provider =>
			new NotificationDefinitionBase<TNotification, TModel>(
				systemName,
				provider.GetRequiredService<INotificationSerializer>(),
				createNotification));

		return services;
	}
}
