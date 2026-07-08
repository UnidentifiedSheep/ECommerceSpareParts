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
}