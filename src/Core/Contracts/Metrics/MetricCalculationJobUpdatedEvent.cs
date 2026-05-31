using Abstractions.Interfaces;

namespace Contracts.Metrics;

public record MetricCalculationJobUpdatedEvent : IKeyedEvent
{
    public required Guid? MetricId { get; init; }
    public required Guid RequestId { get; init; }
    public required string CalculationStatus { get; init; }
    public required string? ErrorMessage { get; init; }
    public string GetKey() => $"metric-calculation-job-updated:{RequestId}";
}