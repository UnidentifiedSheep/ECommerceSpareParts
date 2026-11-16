using Main.Modules.Notifications.Application.Configs;
using Application.Common.Host;
using Main.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Notification.Core.Interfaces.Recipient;

namespace Main.Modules.Notifications.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNotificationsApplication(this IServiceCollection services)
	{
		NotificationsCursorConfig.Configure();

		services.AddModuleApplication(NotificationsApplicationAssembly.Value);
		services.AddScoped<IRecipientProvider, RecipientProvider>();
		services.AddScoped<IRecipientResolver>(provider => provider.GetRequiredService<IRecipientProvider>());

		return services;
	}
}
