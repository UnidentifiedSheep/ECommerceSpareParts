using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Main.Entities.DomainEvents.User;
using Main.Modules.Identity.Abstractions;

namespace Main.Application.DomainEventHandlers.User.UserEmailUpdated;

public class PublishUserEmailChangedHandler(IDomainEventScope eventScope)
	: BatchableDomainEventHandler<UserEmailUpdatedDomainEvent>
{
	public override Task Handle(
		Batch<UserEmailUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		foreach (var userId in notification.Items.Select(item => item.UserId).Distinct())
			eventScope.Add(new UserEmailChanged(userId));

		return Task.CompletedTask;
	}
}
