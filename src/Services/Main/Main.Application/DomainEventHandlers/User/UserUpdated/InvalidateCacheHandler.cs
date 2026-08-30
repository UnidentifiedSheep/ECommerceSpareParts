using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Application.Interfaces.Cache;
using Main.Entities.DomainEvents.User;

namespace Main.Application.DomainEventHandlers.User.UserUpdated;

public class InvalidateCacheHandler(IUserCacheRepository userCache)
	: BatchableDomainEventHandler<UserUpdatedDomainEvent>
{
	public override Task Handle(
		Batch<UserUpdatedDomainEvent> notification,
		CancellationToken cancellationToken) => userCache.InvalidateUsersAsync(
		notification.Items.Select(x => x.UserId).ToList());
}
