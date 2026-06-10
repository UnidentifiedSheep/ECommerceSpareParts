using System.Text.Json.Serialization;
using Attributes;
using Attributes.JsonAttributes;

namespace Analytics.Entities.Metrics.JsonDataModels;

public record ProductInfoModel
{
    [JsonPropertyName("price_info")]
    [LocalizedJsonFieldName("price_info")]
    public required PriceInfoModel PriceInfo { get; set; }

    [JsonPropertyName("timer")]
    [LocalizedJsonFieldName("timer")]
    public required MetricTimer Timer { get; set; }

    [JsonPropertyName("quantity")]
    [LocalizedJsonFieldName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("total_amount")]
    [LocalizedJsonFieldName("total_amount")]
    public decimal TotalAmount { get; set; }
}