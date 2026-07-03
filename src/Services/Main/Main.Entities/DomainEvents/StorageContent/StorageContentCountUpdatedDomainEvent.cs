using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.StorageContent;

public record StorageContentCountUpdatedDomainEvent(
    int ProductId,
    string StorageName,
    int Delta
    ) : IBatchableDomainEvent;