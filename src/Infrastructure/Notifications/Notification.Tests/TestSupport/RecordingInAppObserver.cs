using Notification.Core.Interfaces;
using Notification.Core.Recipients;

namespace Notification.Tests.TestSupport;

internal sealed class RecordingInAppObserver(
	Exception? error = null,
	Action<IReadOnlyCollection<InAppReceipt>>? onObserve = null,
	bool throwOnFailure = false) : IChannelDeliveryObserver<InAppReceipt, InAppRecipient>
{
	public bool ThrowOnFailure => throwOnFailure;
	public List<IReadOnlyCollection<InAppReceipt>> Received { get; } = [];
	public CancellationToken CancellationToken { get; private set; }

	public Task ObserveAsync(
		IReadOnlyCollection<InAppReceipt> receipts,
		CancellationToken cancellationToken)
	{
		Received.Add(receipts);
		CancellationToken = cancellationToken;
		onObserve?.Invoke(receipts);
		if (error is not null) throw error;
		return Task.CompletedTask;
	}
}
