using System.Text.Json.Serialization;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Analytics.Application.NamedObjects.Metrics.MetricInputBases;

public record ProductMetricInputBase : MetricInputBase
{
    [JsonPropertyName("productId")]
    [SchemaInputControl(InputControlType.EntitySelector)]
    [RequiredSchemaField]
    [SchemaDependsOnEntity("Product", "id")]
    public int ProductId { get; init; }
}