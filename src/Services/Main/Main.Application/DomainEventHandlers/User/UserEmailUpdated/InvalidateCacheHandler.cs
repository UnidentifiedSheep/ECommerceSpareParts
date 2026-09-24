using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Application.Interfaces.Cache;
using Main.Entities.DomainEvents.User;

namespace Main.Application.DomainEventHandlers.User.UserEmailUpdated;

public class InvalidateCacheHandler(IRecipientProvider recipientProvider)
	: BatchableDomainEventHandler<UserEmailUpdatedDomainEvent>
{
	public override Task Handle(
		Batch<UserEmailUpdatedDomainEvent> notification,
		CancellationToken cancellationToken) =>
		recipientProvider.InvalidateUsersRecipients(notification.Items.Select(item => item.UserId));
}
