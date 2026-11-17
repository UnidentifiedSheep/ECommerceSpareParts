using Main.Modules.Notifications.Application.Delivery.Channels;
using Main.Modules.Notifications.Application.Delivery.Dequeuers;
using Main.Modules.Notifications.Application.Delivery.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NamedObject;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Recipients;

namespace Main.Modules.Notifications.Application.Delivery.Extensions;

public static class InAppNotificationServiceCollectionExtensions
{
	public static IServiceCollection UseInAppNotifications(this IServiceCollection services)
	{
		services.AddNamedObjectRegistry();
		services.TryAddEnumerable(ServiceDescriptor.Scoped<INotificationChannel, InAppChannel>());

		services.AddOptions<InAppChannelOptions>()
			.BindConfiguration(InAppChannelOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		return services;
	}

	public static IServiceCollection AddInAppNotificationHostedService(this IServiceCollection services)
	{
		services.AddSingleton<Dequeuer<InAppRecipient>>(sp =>
			new Dequeuer<InAppRecipient>(
				InAppRecipient.ChannelName,
				sp.GetRequiredService<ILogger<Dequeuer<InAppRecipient>>>(),
				sp.GetRequiredService<IOptions<InAppChannelOptions>>().Value,
				sp.GetRequiredService<IServiceScopeFactory>()));

		services.AddHostedService(sp => sp.GetRequiredService<Dequeuer<InAppRecipient>>());
		return services;
	}
}
