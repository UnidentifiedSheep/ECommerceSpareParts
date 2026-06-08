using System.Text.Json.Serialization;
using Attributes;
using Attributes.JsonAttributes;

namespace Analytics.Entities.Metrics.JsonDataModels;

public record DatedPriceModel
{
    [JsonPropertyName("date")]
    [LocalizedJsonFieldName("date")]
    public required DateTime Date { get; init; }

    [JsonPropertyName("quantity")]
    [LocalizedJsonFieldName("quantity")]
    public required int Quantity { get; init; }

    [JsonPropertyName("price")]
    [LocalizedJsonFieldName("price")]
    public required decimal Price { get; init; }
}