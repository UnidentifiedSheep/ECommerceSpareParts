using Notification.Core.Interfaces.Notification;

namespace Notification.Renderers;

public sealed record NotificationContent(string Text) : INotificationContent;
