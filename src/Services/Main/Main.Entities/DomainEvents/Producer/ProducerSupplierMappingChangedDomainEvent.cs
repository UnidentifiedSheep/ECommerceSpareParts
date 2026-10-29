using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Producer;

public sealed record ProducerSupplierMappingChangedDomainEvent(int ProducerId) : IBatchableDomainEvent, IKeyedDomainEvent
{
	public string GetKey() => $"producer:{ProducerId}:supplier-mappings:changed";
}
