using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Application.Interfaces.Cache;
using Main.Entities.DomainEvents.User;

namespace Main.Application.DomainEventHandlers.User.UserNotificationPreferenceUpdated;

public class InvalidateCacheHandler(IRecipientProvider recipientProvider)
	: BatchableDomainEventHandler<UserNotificationPreferenceUpdatedDomainEvent>
{
	public override Task Handle(
		Batch<UserNotificationPreferenceUpdatedDomainEvent> notification,
		CancellationToken cancellationToken) =>
		recipientProvider.InvalidateUsersRecipients(notification.Items.Select(item => item.UserId));
}
