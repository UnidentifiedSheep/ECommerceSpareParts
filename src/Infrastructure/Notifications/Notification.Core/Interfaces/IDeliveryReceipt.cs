using Notification.Core.Interfaces.Recipient;

namespace Notification.Core.Interfaces;

public interface IDeliveryReceipt<out TRecipient> where TRecipient : INotificationRecipient
{
	TRecipient Recipient { get; }
}
