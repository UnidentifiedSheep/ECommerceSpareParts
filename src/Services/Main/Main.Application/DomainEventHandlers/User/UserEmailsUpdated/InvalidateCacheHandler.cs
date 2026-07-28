using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Main.Application.Interfaces.Cache;
using Main.Entities.DomainEvents.User;

namespace Main.Application.DomainEventHandlers.User.UserEmailsUpdated;

public class InvalidateCacheHandler(
    IUserCacheRepository userCache) : BatchableDomainEventHandler<UserEmailsUpdatedDomainEvent>
{
    public override Task Handle(
        Batch<UserEmailsUpdatedDomainEvent> notification, 
        CancellationToken cancellationToken)
        => userCache.InvalidateUsersAsync(
            notification.Items
                .Select(x => x.UserId)
                .ToList());
}