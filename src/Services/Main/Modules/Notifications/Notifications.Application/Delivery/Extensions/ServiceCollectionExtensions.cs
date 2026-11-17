using Main.Modules.Notifications.Application.Delivery.Channels;
using Main.Modules.Notifications.Application.Delivery.Channels.Email;
using Main.Modules.Notifications.Application.Delivery.Interfaces;
using Main.Modules.Notifications.Application.Delivery.Renderers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NamedObject;
using Notification.Core.Interfaces.Notification;

namespace Main.Modules.Notifications.Application.Delivery.Extensions;

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
