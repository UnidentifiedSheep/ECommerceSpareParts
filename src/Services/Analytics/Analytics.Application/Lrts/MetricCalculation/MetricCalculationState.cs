using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using Attributes.JsonAttributes;
using Enums;

namespace Analytics.Application.Lrts.MetricCalculation;

public record MetricCalculationState
{
    [JsonPropertyName("metricId")]
    public required Guid MetricId { get; init; }
}

public record MetricCalculationInputState : IInputState
{
    [InputControl(InputControlType.TextField)]
    [RequiredJsonField]
    [LocalizedJsonFieldDescription("metric.id.description")]
    [LocalizedJsonFieldName("metric.id.name")]
    [JsonPropertyName("metricId")]
    public required Guid MetricId { get; init; }

    public void ValidateState() { }
}