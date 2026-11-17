using Contracts.Interfaces;

namespace Main.Modules.Product.Application.Events;

public sealed record ProducerLookupChangedEvent : IKeyedEvent
{
	public string GetKey() => "producer-lookup:changed";
}
