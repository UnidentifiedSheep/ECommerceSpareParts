using System.Text.Json.Serialization;

namespace Pricing.Application.Models.Pricing.MarketInfo;

public record MarketInfoItem
{
    [JsonPropertyName("costInBaseCurrency")]
    public decimal CostInBaseCurrency { get; init; }

    [JsonPropertyName("deliveryTime")]
    public TimeSpan DeliveryTime { get; init; }

    [JsonPropertyName("score")]
    public decimal Score { get; init; }
}
