using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NamedObject;
using Notification.Channels;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Recipients;
using Notification.Dequeuers;
using Notification.Options;

namespace Notification.Extensions;

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
				systemName: InAppRecipient.ChannelName,
				logger: sp.GetRequiredService<ILogger<Dequeuer<InAppRecipient>>>(),
				options: sp.GetRequiredService<IOptions<InAppChannelOptions>>().Value,
				scopeFactory: sp.GetRequiredService<IServiceScopeFactory>()));

		services.AddHostedService(sp => sp.GetRequiredService<Dequeuer<InAppRecipient>>());
		return services;
	}
}
