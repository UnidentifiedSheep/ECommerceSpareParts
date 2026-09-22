using System.Reflection;
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

	public static IServiceCollection AddNotificationDefinitions(
		this IServiceCollection services,
		params Assembly[] assemblies)
	{
		ArgumentNullException.ThrowIfNull(assemblies);

		if (assemblies.Length == 0)
			throw new ArgumentException(
				"At least one notification definitions assembly must be specified.",
				nameof(assemblies));

		foreach (var assembly in assemblies.Distinct())
		{
			ArgumentNullException.ThrowIfNull(assembly);
			services.RegisterNamedObject<INotificationDefinition>(assembly);
		}

		return services;
	}
}
