using Abstractions.Interfaces;

namespace Contracts.Producer;

public class ProducerUpdatedEvent : IKeyedEvent
{
    public required int Id { get; init; }
    public string GetKey() => $"producer-updated:{Id}";
}