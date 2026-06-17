using Abstractions.Interfaces;

namespace Contracts.Job;

public record JobStatusUpdatedEvent : IKeyedEvent
{
    public required Guid JobId { get; init; }
    public required string Status { get; init; }
    public required int CurrentAttempt { get; init; }
    public string GetKey() => $"job-status-updated:{JobId}";
}