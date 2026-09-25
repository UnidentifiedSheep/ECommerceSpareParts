using Application.Common.Interfaces.Events;
using Contracts;
using Notification.Core.Interfaces;
using Notification.Core.Recipients;

namespace Main.Application.Notifications.DeliveryObservers;

public class InAppDeliveryObserver(
	IIntegrationEventScope eventScope
	) : IChannelDeliveryObserver<InAppReceipt, InAppRecipient>
{
	public Task ObserveAsync(IReadOnlyCollection<InAppReceipt> receipts, CancellationToken cancellationToken)
	{
		foreach (var i in receipts)
			eventScope.Add(new InAppNotificationCreatedEvent
			{
				Id = i.CreatedRowId,
				UserId = i.Recipient.UserId
			});
		return Task.CompletedTask;
	}
}
