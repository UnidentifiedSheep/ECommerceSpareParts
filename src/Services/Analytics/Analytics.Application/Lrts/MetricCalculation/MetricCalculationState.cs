using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Analytics.Application.Lrts.MetricCalculation;

public record MetricCalculationState : MetricCalculationInputState;

public record MetricCalculationInputState : IInputState
{
    [SchemaInputControl(InputControlType.TextField)]
    [RequiredSchemaField]
    [JsonPropertyName("metricId")]
    public required Guid MetricId { get; init; }

    public void ValidateState() { }
}
