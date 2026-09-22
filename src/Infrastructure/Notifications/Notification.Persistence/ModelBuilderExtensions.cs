using Microsoft.EntityFrameworkCore;

namespace Notification.Persistence;

public static class ModelBuilderExtensions
{
	public static ModelBuilder AddNotifications(this ModelBuilder builder)
		=> builder.ApplyConfiguration(new NotificationConfiguration())
			.ApplyConfiguration(new NotificationDeliveryConfiguration())
			.ApplyConfiguration(new InAppNotificationConfiguration());
}
