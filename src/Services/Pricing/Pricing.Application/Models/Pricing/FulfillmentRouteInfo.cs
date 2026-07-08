using Pricing.Entities;

namespace Pricing.Application.Models.Pricing;

public sealed record FulfillmentRouteInfo(
    string SourceStorageName,
    string TargetStorageName,
    decimal LogisticsCostInBaseCurrency,
    TimeSpan DeliveryTime,
    TimeSpan GuaranteedDeliveryTime,
    int DeliveryProbability)
{
    public static FulfillmentRouteInfo SameStorage(string storageName)
    {
        return new FulfillmentRouteInfo(
            SourceStorageName: storageName,
            TargetStorageName: storageName,
            LogisticsCostInBaseCurrency: 0,
            DeliveryTime: TimeSpan.Zero,
            GuaranteedDeliveryTime: TimeSpan.Zero,
            DeliveryProbability: 100);
    }

    public static FulfillmentRouteInfo FromSupplier(PriceOffer offer)
    {
        return new FulfillmentRouteInfo(
            SourceStorageName: offer.OfferForStorage,
            TargetStorageName: offer.OfferForStorage,
            LogisticsCostInBaseCurrency: 0,
            DeliveryTime: offer.DeliveryDate - offer.UpdatedAt,
            GuaranteedDeliveryTime: offer.GuaranteedDeliveryDate - offer.UpdatedAt,
            DeliveryProbability: offer.DeliveryProbability);
    }
}