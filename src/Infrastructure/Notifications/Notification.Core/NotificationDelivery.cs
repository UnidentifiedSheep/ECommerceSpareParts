using Notification.Core.Interfaces;

namespace Notification.Core;

public record NotificationDelivery<TNotification, TRecipient>(
	TNotification Notification,
	TRecipient Recipient)
	where TNotification : INotification
	where TRecipient : INotificationRecipient;
