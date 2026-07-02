using Abstractions.Interfaces;
using Abstractions.Interfaces.Events;

namespace Contracts.Producer;

public class ProducerUpdatedEvent : IKeyedEvent
{
    public required int Id { get; init; }
    public string GetKey() { return $"producer-updated:{Id}"; }
}