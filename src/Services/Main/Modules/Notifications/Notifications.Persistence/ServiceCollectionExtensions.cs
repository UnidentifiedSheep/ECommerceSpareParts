using Notification.Persistence;
using Persistence.Context;
using Persistence.Common;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Notifications.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNotificationsPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<NotificationsDbContextModule>().AddEntities(typeof(Main.Entities.User.UserNotificationPreference).Assembly, typeof(Notification.Core.Entities.InAppNotification).Assembly).AddNotificationPersistence<UnifiedDbContext>();
}
