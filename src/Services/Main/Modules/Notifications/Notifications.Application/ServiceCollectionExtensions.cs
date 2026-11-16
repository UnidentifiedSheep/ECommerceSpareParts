using Main.Modules.Notifications.Application.Configs;
using Application.Common.Host;
using Main.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Main.Application.Notifications.DeliveryObservers;
using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Recipient;
using Notification.Core.Recipients;
using Notification.Extensions;

namespace Main.Modules.Notifications.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNotificationsApplication(this IServiceCollection services)
	{
		NotificationsCursorConfig.Configure();

		services.AddModuleApplication(NotificationsApplicationAssembly.Value);
		services.AddNotificationServices();
		services.AddScoped<IChannelDeliveryObserver<InAppReceipt, InAppRecipient>, InAppDeliveryObserver>();
		services.AddScoped<IRecipientProvider, RecipientProvider>();
		services.AddScoped<IRecipientResolver>(provider => provider.GetRequiredService<IRecipientProvider>());

		return services;
	}
}
