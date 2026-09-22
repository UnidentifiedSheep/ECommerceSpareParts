using Notification.Core.Interfaces;

namespace Notification.Core;

public record NotificationItem(Guid UserId, INotification Notification);
