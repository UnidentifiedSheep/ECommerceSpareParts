using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Notifications.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNotificationsPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<NotificationsDbContextModule>();
}
