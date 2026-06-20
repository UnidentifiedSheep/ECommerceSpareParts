using System.Text.Json;
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
    [LocalizedJsonFieldDescription("metric_id_description")]
    [LocalizedJsonFieldName("metric_id_name")]
    [JsonPropertyName("metricId")]
    public required Guid MetricId { get; init; }

    public static string GetAndValidateState(string jsonState)
    {
        _ = JsonSerializer.Deserialize<MetricCalculationState>(jsonState)
                    ?? throw new InvalidOperationException("Invalid state");
        
        return jsonState;
    }
}