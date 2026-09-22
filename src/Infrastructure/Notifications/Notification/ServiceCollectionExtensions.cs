using Locan.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NamedObject;
using Notification.Core.Interfaces;
using Notification.Channels;
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
}
