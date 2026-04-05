using Contracts.Models.Metric;

namespace Contracts.Analytics;

public record MetricCalculationRequestedEvent
{
    public required Guid RequestId { get; init; }
    public required string MetricSystemName { get; init; }
    public required MetricPayloadDto MetricPayload { get; init; }
    public required Guid CreatedBy { get; init; }
}