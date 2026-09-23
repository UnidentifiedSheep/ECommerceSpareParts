using Notification.Core.Interfaces.Recipient;

namespace Notification.Core.Interfaces;

public interface IChannelDeliveryObserver<TReceipt, TRecipient>
	where TReceipt : IDeliveryReceipt<TRecipient>
	where TRecipient : INotificationRecipient
{
	bool ThrowOnFailure { get; }
	Task ObserveAsync(
		IReadOnlyCollection<TReceipt> receipts,
		CancellationToken cancellationToken);
}
