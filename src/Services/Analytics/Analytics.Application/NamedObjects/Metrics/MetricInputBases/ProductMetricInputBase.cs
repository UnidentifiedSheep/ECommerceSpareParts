using System.Text.Json.Serialization;
using Attributes.JsonAttributes;
using Enums;

namespace Analytics.Application.NamedObjects.Metrics.MetricInputBases;

public record ProductMetricInputBase : MetricInputBase
{
    [JsonPropertyName("productId")]
    [InputControl(InputControlType.EntitySelector)]
    [RequiredJsonField]
    [DependsOnEntity("Product", "id")]
    [LocalizedJsonFieldDescription("metric.input.field.product.id.description")]
    [LocalizedJsonFieldName("metric.input.field.product.id.name")]
    public int ProductId { get; init; }
}