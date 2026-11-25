using Domain.Interfaces.Events;
using Enums;

namespace Main.Entities.Logistics.DomainEvents;

public sealed class DeliveryScheduleChangedDomainEvent(Supplier supplier, string storageCode)
	: IKeyedDomainEvent, IBatchableDomainEvent
{
	public Supplier Supplier { get; } = supplier;

	public string StorageCode { get; } = storageCode;

	public string GetKey() => $"delivery-schedule:{Supplier}:{StorageCode}:changed";
}
