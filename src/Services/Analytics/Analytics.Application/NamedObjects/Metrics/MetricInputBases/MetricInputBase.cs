using System.Text.Json.Serialization;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Analytics.Application.NamedObjects.Metrics.MetricInputBases;

public record MetricInputBase
{
    [JsonPropertyName("currencyId")]
    [SchemaInputControl(InputControlType.EntitySelector)]
    [RequiredSchemaField]
    [SchemaDependsOnEntity("Currency", "id")]
    public required int CurrencyId { get; init; }

    [JsonPropertyName("rangeStart")]
    [SchemaInputControl(InputControlType.DatePicker)]
    [RequiredSchemaField]
    public required DateTime RangeStart { get; init; }

    [JsonPropertyName("rangeEnd")]
    [SchemaInputControl(InputControlType.DatePicker)]
    [RequiredSchemaField]
    public required DateTime RangeEnd { get; init; }
}