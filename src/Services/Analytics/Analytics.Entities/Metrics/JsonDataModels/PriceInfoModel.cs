using System.Text.Json.Serialization;
using Attributes;
using Attributes.JsonAttributes;

namespace Analytics.Entities.Metrics.JsonDataModels;

public record PriceInfoModel
{
    [JsonPropertyName("min_price")]
    [LocalizedJsonFieldName("min_price")]
    public required decimal MinimumPrice { get; init; }

    [JsonPropertyName("max_price")]
    [LocalizedJsonFieldName("max_price")]
    public required decimal MaximumPrice { get; init; }

    [JsonPropertyName("average_price")]
    [LocalizedJsonFieldName("average_price")]
    public required decimal AveragePrice { get; init; }

    [JsonPropertyName("volatility")]
    [LocalizedJsonFieldName("volatility")]
    public required decimal Volatility { get; init; }
}