using Contracts.Interfaces;

namespace Contracts.Producer;

public sealed record ProducerLookupChangedEvent : IKeyedEvent
{
	public string GetKey() => "producer-lookup:changed";
}
