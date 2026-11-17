using System.Text.Json.Serialization;
using Pricing.Entities.Offers;

namespace Pricing.Application.Models.Pricing;

public sealed record FulfillmentRouteInfo(
	[property: JsonPropertyName("sourceStorageCode")]
	string SourceStorageCode,
	[property: JsonPropertyName("targetStorageCode")]
	string TargetStorageCode,
	[property: JsonPropertyName("logisticsCostInBaseCurrency")]
	decimal LogisticsCostInBaseCurrency,
	[property: JsonPropertyName("deliveryTime")]
	TimeSpan DeliveryTime,
	[property: JsonPropertyName("guaranteedDeliveryTime")]
	TimeSpan GuaranteedDeliveryTime,
	[property: JsonPropertyName("deliveryProbability")]
	int DeliveryProbability)
{
	public static FulfillmentRouteInfo SameStorage(string storageCode)
		=> new(
			storageCode,
			storageCode,
			0,
			TimeSpan.Zero,
			TimeSpan.Zero,
			100);

	public static FulfillmentRouteInfo FromSupplier(PriceOffer offer)
		=> new(
			offer.OfferForStorage,
			offer.OfferForStorage,
			0,
			offer.DeliveryDate == null ? TimeSpan.Zero : offer.DeliveryDate.Value - offer.UpdatedAt,
			offer.GuaranteedDeliveryDate == null
				? TimeSpan.Zero
				: offer.GuaranteedDeliveryDate.Value - offer.UpdatedAt,
			offer.DeliveryProbability);
}
