using Domain.Interfaces.Events;
using Main.Enums;

namespace Main.Entities.DomainEvents.StorageContent;

public record StorageContentCountUpdatedDomainEvent(
    int ProductId,
    string StorageName,
    int CurrencyId,
    int NewCount,
    decimal BuyPrice,
    StorageMovementType MovementType,
    int Delta
    ) : IBatchableDomainEvent;