using System.Text.Json.Serialization;
using Attributes;

namespace Analytics.Entities.Metrics.JsonDataModels;

public record DatedPriceModel
{
    [JsonPropertyName("date")]
    [LocalizableJsonPropertyName("date")]
    public required DateTime Date { get; init; }

    [JsonPropertyName("quantity")]
    [LocalizableJsonPropertyName("quantity")]
    public required int Quantity { get; init; }

    [JsonPropertyName("price")]
    [LocalizableJsonPropertyName("price")]
    public required decimal Price { get; init; }
}