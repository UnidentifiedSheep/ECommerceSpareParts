using Main.Entities.Notification;
using Main.Entities.User;
using Main.Modules.Notifications.Persistence.Delivery;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Common;
using Persistence.Context;
using Persistence.Extensions;

namespace Main.Modules.Notifications.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNotificationsPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<NotificationsDbContextModule>()
			.AddEntities(typeof(UserNotificationPreference).Assembly, typeof(InAppNotification).Assembly)
			.AddNotificationPersistence<UnifiedDbContext>();
}
