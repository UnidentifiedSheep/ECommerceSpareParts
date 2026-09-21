using Notification.Core.Interfaces;

namespace Notification.NotificationContents;

public sealed record TextNotificationContent(string Text) : INotificationContent;
