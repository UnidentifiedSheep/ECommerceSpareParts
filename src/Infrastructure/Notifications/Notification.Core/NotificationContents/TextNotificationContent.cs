using Notification.Core.Interfaces;

namespace Notification.Core.NotificationContents;

public sealed record TextNotificationContent(string Text) : INotificationContent;
