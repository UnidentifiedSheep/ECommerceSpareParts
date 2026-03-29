using System.Text.Json.Serialization;

namespace Analytics.Entities.Metrics.JsonDataModels;

public record ArticleInfoModel
{
    [JsonPropertyName("price_info")]
    public required PriceInfoModel PriceInfo { get; set; }

    [JsonPropertyName("timer")]
    public required MetricTimer Timer { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("total_amount")]
    public decimal TotalAmount { get; set; }
}