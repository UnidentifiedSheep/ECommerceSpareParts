using System.Text.Json.Serialization;
using Pricing.Entities.Enums;

namespace Pricing.Application.Models.Pricing.PriceCandidates;

public sealed record PriceCandidate(
	[property: JsonPropertyName("priceOfferId")]
	Guid PriceOfferId,
	[property: JsonPropertyName("productId")]
	int ProductId,
	[property: JsonPropertyName("targetStorageCode")]
	string TargetStorageCode,
	[property: JsonPropertyName("sourceType")]
	PriceOfferSourceType SourceType,
	[property: JsonPropertyName("cost")]
	decimal Cost,
	[property: JsonPropertyName("currencyId")]
	int CurrencyId,
	[property: JsonPropertyName("availableQuantity")]
	int AvailableQuantity,
	[property: JsonPropertyName("deliveryDays")]
	int DeliveryDays,
	[property: JsonPropertyName("guaranteedDeliveryDays")]
	int GuaranteedDeliveryDays,
	[property: JsonPropertyName("deliveryProbability")]
	int DeliveryProbability);
