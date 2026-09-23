using Microsoft.EntityFrameworkCore;
using Notification.Core.Entities;
using NotificationDelivery = Notification.Core.NotificationDelivery;

namespace Notification.Persistence.Interfaces;

public interface INotificationDbContext
{
	DbSet<NotificationDelivery> NotificationDeliveries { get; }
	DbSet<Core.Entities.Notification> Notifications { get; }
	DbSet<InAppNotification> InAppNotifications { get; }
}
