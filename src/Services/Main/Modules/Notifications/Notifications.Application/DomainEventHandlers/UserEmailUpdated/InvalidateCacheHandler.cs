using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Application.Services;
using Main.Modules.Identity.Abstractions;

namespace Main.Application.DomainEventHandlers.User.UserEmailUpdated;

public class InvalidateCacheHandler(IRecipientProvider recipientProvider)
	: BatchableDomainEventHandler<UserEmailChanged>
{
	public override Task Handle(
		Batch<UserEmailChanged> notification,
		CancellationToken cancellationToken)
		=> recipientProvider.InvalidateUsersRecipients(notification.Items.Select(item => item.UserId));
}
