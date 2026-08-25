using System.Text.Json.Serialization;
using Pricing.Entities;
using Pricing.Entities.Offers;

namespace Pricing.Application.Models.Pricing;

public sealed record FulfillmentRouteInfo(
    [property: JsonPropertyName("sourceStorageCode")] string SourceStorageCode,
    [property: JsonPropertyName("targetStorageCode")] string TargetStorageCode,
    [property: JsonPropertyName("logisticsCostInBaseCurrency")] decimal LogisticsCostInBaseCurrency,
    [property: JsonPropertyName("deliveryTime")] TimeSpan DeliveryTime,
    [property: JsonPropertyName("guaranteedDeliveryTime")] TimeSpan GuaranteedDeliveryTime,
    [property: JsonPropertyName("deliveryProbability")] int DeliveryProbability)
{
    public static FulfillmentRouteInfo SameStorage(string storageCode)
    {
        return new FulfillmentRouteInfo(
            SourceStorageCode: storageCode,
            TargetStorageCode: storageCode,
            LogisticsCostInBaseCurrency: 0,
            DeliveryTime: TimeSpan.Zero,
            GuaranteedDeliveryTime: TimeSpan.Zero,
            DeliveryProbability: 100);
    }

    public static FulfillmentRouteInfo FromSupplier(PriceOffer offer)
    {
        return new FulfillmentRouteInfo(
            SourceStorageCode: offer.OfferForStorage,
            TargetStorageCode: offer.OfferForStorage,
            LogisticsCostInBaseCurrency: 0,
            DeliveryTime: offer.DeliveryDate == null 
                ? TimeSpan.Zero 
                : offer.DeliveryDate.Value - offer.UpdatedAt,
            GuaranteedDeliveryTime: offer.GuaranteedDeliveryDate == null
                ? TimeSpan.Zero 
                : offer.GuaranteedDeliveryDate.Value - offer.UpdatedAt,
            DeliveryProbability: offer.DeliveryProbability);
    }
}
