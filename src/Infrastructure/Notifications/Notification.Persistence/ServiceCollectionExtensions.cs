using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;

namespace Notification.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNotificationPersistence(this IServiceCollection services)
	{
		services.TryAddScoped<INotificationStore, NotificationStore>();

		return services;
	}
}
