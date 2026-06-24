using System.Text.Json.Serialization;
using Attributes.JsonAttributes;
using Enums;

namespace Analytics.Application.NamedObjects.Metrics.MetricInputBases;

public record MetricInputBase
{
    [JsonPropertyName("currencyId")]
    [InputControl(InputControlType.EntitySelector)]
    [RequiredJsonField]
    [DependsOnEntity("Currency", "id")]
    [LocalizedJsonFieldDescription("metric.input.field.currency.id.description")]
    [LocalizedJsonFieldName("metric.input.field.currency.id.name")]
    public required int CurrencyId { get; init; }
    
    [JsonPropertyName("rangeStart")]
    [InputControl(InputControlType.DatePicker)]
    [RequiredJsonField]
    [LocalizedJsonFieldDescription("datetime.range.start.description")]
    [LocalizedJsonFieldName("datetime.range.start.name")]
    public required DateTime RangeStart { get; init; }
    
    [JsonPropertyName("rangeEnd")]
    [InputControl(InputControlType.DatePicker)]
    [RequiredJsonField]
    [LocalizedJsonFieldDescription("datetime.range.end.description")]
    [LocalizedJsonFieldName("datetime.range.end.name")]
    public required DateTime RangeEnd { get; init; }
}
