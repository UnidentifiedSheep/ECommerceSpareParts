using Abstractions.Interfaces;

namespace Contracts.Analytics;

public class MetricCalculationStatusUpdatedEvent : IKeyedEvent
{
    public required Guid MetricId { get; init; }
    public required string JobStatus { get; init; }
    public string GetKey() => $"metric-calculation-status-updated:{MetricId}";
}