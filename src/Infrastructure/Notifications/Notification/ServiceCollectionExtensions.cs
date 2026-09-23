using System.Globalization;
using Locan.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NamedObject;
using Notification.Channels;
using Notification.Core;
using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;
using Notification.Interfaces;
using Notification.NotificationContents;
using Notification.Renderers;

namespace Notification;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNotificationServices(this IServiceCollection services)
	{
		services.RegisterNamedObject<INotificationChannel>(typeof(InAppChannel).Assembly);

		services.TryAddScoped<
			INotificationRenderer<ISimpleNotification<ILocalizableMessage>, TextNotificationContent>,
			TextNotificationRenderer>();
		services.TryAddSingleton<INotificationSerializer, NotificationSerializer>();
		services.TryAddScoped<INotificationService, NotificationService>();

		return services;
	}

	public static IServiceCollection AddNotification<TNotification, TModel>(
		this IServiceCollection services,
		string systemName,
		Func<TModel, CultureInfo?, TNotification> createNotification)
		where TNotification : INotification<TModel>
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
