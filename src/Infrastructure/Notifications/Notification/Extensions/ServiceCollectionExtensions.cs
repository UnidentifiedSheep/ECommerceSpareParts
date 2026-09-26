using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NamedObject;
using Notification.Channels;
using Notification.Channels.Email;
using Notification.Core;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Interfaces.Recipient;
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

		AddRenderers(services);

		services.TryAddSingleton<INotificationSerializer, NotificationSerializer>();
		services.TryAddScoped<INotificationService, NotificationService>();

		services.Add(
			new ServiceDescriptor(
				typeof(INotificationService),
				typeof(NotificationService),
				serviceLifetime));

		return services;
	}

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

	private static void AddRenderers(this IServiceCollection services)
	{
		services.TryAddSingleton<
			INotificationRenderer<ITextNotification>,
			TextNotificationRenderer>();

		services.TryAddSingleton<INotificationRenderer<INotification>, HtmlNotificationRenderer>();
	}
}
