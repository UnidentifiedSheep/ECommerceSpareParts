using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Modules.Identity.Abstractions.Events;
using Main.Modules.Notifications.Application.Services;

namespace Main.Modules.Notifications.Application.DomainEventHandlers.UserEmailUpdated;

public class InvalidateCacheHandler(IRecipientProvider recipientProvider)
	: BatchableDomainEventHandler<UserEmailChanged>
{
	public override Task Handle(
		Batch<UserEmailChanged> notification,
		CancellationToken cancellationToken)
		=> recipientProvider.InvalidateUsersRecipients(notification.Items.Select(item => item.UserId));
}
