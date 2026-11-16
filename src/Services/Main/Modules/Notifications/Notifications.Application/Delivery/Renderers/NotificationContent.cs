using Notification.Core.Interfaces.Notification;

namespace Main.Modules.Notifications.Application.Delivery.Renderers;

public sealed record NotificationContent(string Text, string Title = "") : INotificationContent;
