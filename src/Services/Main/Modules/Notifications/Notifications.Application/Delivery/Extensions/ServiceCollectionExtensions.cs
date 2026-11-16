using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NamedObject;
using Notification.Channels;
using Notification.Channels.Email;
using Notification.Core;
using Notification.Core.Interfaces.Notification;
using Notification.Interfaces;
using Notification.Renderers;

namespace Notification.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNotificationServices(
		this IServiceCollection services,
		ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
	{
		services.RegisterNamedObject<INotificationChannel>(
			typeof(InAppChannel).Assembly,
			objectsToExclude: [typeof(EmailChannel)]);
		services.UseInAppNotifications();

		services.AddRenderers();

		services.TryAddSingleton<INotificationSerializer, NotificationSerializer>();
		services.TryAddScoped<INotificationService, NotificationService>();

		services.Add(
			new ServiceDescriptor(
				typeof(INotificationService),
				typeof(NotificationService),
				serviceLifetime));

		return services;
	}

	private static void AddRenderers(this IServiceCollection services)
	{
		services.TryAddSingleton<
			INotificationRenderer<ITextNotification>,
			TextNotificationRenderer>();

		services.TryAddSingleton<INotificationRenderer<INotification>, HtmlNotificationRenderer>();
	}
}
