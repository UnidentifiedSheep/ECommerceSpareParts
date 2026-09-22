using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;

namespace Notification.NotificationContents;

public sealed record TextNotificationContent(string Text) : INotificationContent;
