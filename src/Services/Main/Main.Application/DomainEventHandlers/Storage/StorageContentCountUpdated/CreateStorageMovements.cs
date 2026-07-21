using Abstractions.Interfaces.Persistence;
using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Entities.DomainEvents.StorageContent;
using Main.Entities.Event;

namespace Main.Application.DomainEventHandlers.Storage.StorageContentCountUpdated;

public class CreateStorageMovements(
    IUnitOfWork unitOfWork
    ) : BatchableDomainEventHandler<StorageContentCountUpdatedDomainEvent>
{
    public override Task Handle(Batch<StorageContentCountUpdatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var events = notification.Items
            .Where(x => x.Delta != 0)
            .Select(item => new StorageMovementEvent(
                new StorageMovementEventData
                {
                    ProductId = item.ProductId,
                    StorageName = item.StorageName,
                    CurrencyId = item.CurrencyId,
                    Count = item.NewCount,
                    BuyPrice = item.BuyPrice,
                    MovementType = item.MovementType
                })
            )
            .ToList();
        
        return unitOfWork.AddRangeAsync(events, cancellationToken);
    }
}