using System.Text.Json.Serialization;
using Attributes;

namespace Analytics.Entities.Metrics.JsonDataModels;

public record ProductInfoModel
{
    [JsonPropertyName("price_info")]
    [LocalizableJsonPropertyName("price_info")]
    public required PriceInfoModel PriceInfo { get; set; }

    [JsonPropertyName("timer")]
    [LocalizableJsonPropertyName("timer")]
    public required MetricTimer Timer { get; set; }

    [JsonPropertyName("quantity")]
    [LocalizableJsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("total_amount")]
    [LocalizableJsonPropertyName("total_amount")]
    public decimal TotalAmount { get; set; }
}