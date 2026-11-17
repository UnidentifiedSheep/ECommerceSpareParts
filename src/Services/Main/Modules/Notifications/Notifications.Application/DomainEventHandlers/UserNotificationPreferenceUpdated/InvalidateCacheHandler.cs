using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Entities.DomainEvents;
using Main.Modules.Notifications.Application.Services;

namespace Main.Modules.Notifications.Application.DomainEventHandlers.UserNotificationPreferenceUpdated;

public class InvalidateCacheHandler(IRecipientProvider recipientProvider)
	: BatchableDomainEventHandler<UserNotificationPreferenceUpdatedDomainEvent>
{
	public override Task Handle(
		Batch<UserNotificationPreferenceUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
		=> recipientProvider.InvalidateUsersRecipients(notification.Items.Select(item => item.UserId));
}
