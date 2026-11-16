using Domain.Interfaces.Events;

namespace Main.Entities.Storage.DomainEvents.StorageContent;

public record StorageContentUpdatedDomainEvent(Storage.StorageContent Content, bool Deleted)
	: IBatchableDomainEvent;
