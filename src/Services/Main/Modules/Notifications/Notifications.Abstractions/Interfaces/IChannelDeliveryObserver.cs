using Notification.Core.Interfaces.Recipient;

namespace Notification.Core.Interfaces;

public interface IChannelDeliveryObserver<TReceipt, TRecipient>
	where TReceipt : IDeliveryReceipt<TRecipient>
	where TRecipient : INotificationRecipient
{
	Task ObserveAsync(
		IReadOnlyCollection<TReceipt> receipts,
		CancellationToken cancellationToken);
}
