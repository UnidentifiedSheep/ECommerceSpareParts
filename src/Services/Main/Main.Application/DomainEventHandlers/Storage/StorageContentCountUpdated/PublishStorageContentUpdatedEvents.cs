using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Storage;
using Main.Entities.DomainEvents.StorageContent;

namespace Main.Application.DomainEventHandlers.Storage.StorageContentCountUpdated;

public class PublishStorageContentUpdatedEvents(
    IIntegrationEventScope integrationEventScope
    ) : BatchableDomainEventHandler<StorageContentUpdatedDomainEvent>
{
    public override Task Handle(
        Batch<StorageContentUpdatedDomainEvent> notification, 
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        foreach (var item in notification.Items)
        {
            if (item.Content.Id == 0) 
                throw new InvalidOperationException("Storage content must be saved before.");
            integrationEventScope.Add(new StorageContentUpdatedEvent
            {
                OccurredAt = now,
                ProductId = item.Content.ProductId,
                StorageName = item.Content.StorageName,
                StorageContentId = item.Content.Id,
                AvailableCount = item.Deleted ? 0 : item.Content.Count,
                BuyPrice = item.Content.BuyPrice,
                CurrencyId = item.Content.CurrencyId,
            });
        }
        return Task.CompletedTask;
    }
}