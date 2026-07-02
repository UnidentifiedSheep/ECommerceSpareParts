using Abstractions.Interfaces;
using Abstractions.Interfaces.Events;

namespace Contracts.Analytics;

public class MetricCalculationStatusUpdatedEvent : IKeyedEvent
{
    public required Guid MetricId { get; init; }
    public required string JobStatus { get; init; }
    public string GetKey() { return $"metric-calculation-status-updated:{MetricId}"; }
}