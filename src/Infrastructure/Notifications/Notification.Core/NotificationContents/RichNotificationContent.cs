using Notification.Core.Interfaces;

namespace Notification.Core.NotificationContents;

public sealed record RichNotificationContent(string Subject, string Body) : INotificationContent;
