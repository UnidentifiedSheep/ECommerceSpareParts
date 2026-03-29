using System.Text.Json.Serialization;

namespace Analytics.Entities.Metrics.JsonDataModels;

public record PriceInfoModel
{
    [JsonPropertyName("min_price")]
    public required decimal MinimumPrice { get; init; }

    [JsonPropertyName("max_price")]
    public required decimal MaximumPrice { get; init; }

    [JsonPropertyName("average_price")]
    public required decimal AveragePrice { get; init; }
    
    [JsonPropertyName("volatility")]
    public required decimal Volatility { get; init; }
}