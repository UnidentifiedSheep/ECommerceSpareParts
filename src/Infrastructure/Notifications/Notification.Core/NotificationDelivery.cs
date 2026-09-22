using Notification.Core.Interfaces;

namespace Notification.Core;

public sealed record NotificationDelivery(
	INotification Notification,
	INotificationRecipient Recipient);

public record NotificationDelivery<TNotification, TRecipient>(
	TNotification Notification,
	TRecipient Recipient)
	where TNotification : INotification
	where TRecipient : INotificationRecipient;
