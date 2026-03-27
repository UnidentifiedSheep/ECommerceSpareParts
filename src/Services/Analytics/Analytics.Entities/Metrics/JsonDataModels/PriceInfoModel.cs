using System.Text.Json.Serialization;

namespace Analytics.Entities.Metrics.JsonDataModels;

public class PriceInfoModel
{
    [JsonPropertyName("min_price")]
    public decimal MinimumPrice { get; set; }

    [JsonPropertyName("max_price")]
    public decimal MaximumPrice { get; set; }

    [JsonPropertyName("average_price")]
    public decimal AveragePrice { get; set; }
}