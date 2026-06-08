using Abstractions.Interfaces;

namespace Contracts.Lrt;

public record JobQueuedEvent : IKeyedEvent
{
    public required Guid JobId { get; init; }
    public string GetKey() => $"job-queued-{JobId}";
}