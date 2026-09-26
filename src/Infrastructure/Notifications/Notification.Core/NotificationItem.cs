using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;

namespace Notification.Core;

public record NotificationItem(Guid UserId, INotification Notification);
