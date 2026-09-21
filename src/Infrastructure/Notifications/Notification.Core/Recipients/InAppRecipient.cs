using Notification.Core.Interfaces;

namespace Notification.Core.Recipients;

public record InAppRecipient(Guid UserId) : INotificationRecipient;
