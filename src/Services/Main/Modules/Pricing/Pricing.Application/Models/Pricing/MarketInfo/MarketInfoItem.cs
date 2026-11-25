using System.Text.Json.Serialization;

namespace Pricing.Application.Models.Pricing.MarketInfo;

public record MarketInfoItem
{
	[JsonPropertyName("costInBaseCurrency")]
	public decimal CostInBaseCurrency { get; init; }

	[JsonPropertyName("deliveryDays")]
	public int DeliveryDays { get; init; }

	[JsonPropertyName("score")]
	public decimal Score { get; init; }
}
