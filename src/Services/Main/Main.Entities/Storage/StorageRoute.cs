using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Main.Enums;

namespace Main.Entities.Storage;

public class StorageRoute : AuditableEntity<StorageRoute, Guid>
{
    [Validate]
    public Guid Id { get; private set; }

    [ValidateTuple("FromTo")]
    public string FromStorageName { get; private set; } = null!;

    [ValidateTuple("FromTo")]
    public string ToStorageName { get; private set; } = null!;

    public int DistanceM { get; private set; }

    public RouteType RouteType { get; private set; }

    public LogisticPricingType PricingModel { get; private set; }

    public int DeliveryTimeMinutes { get; private set; }

    public decimal PriceKg { get; private set; }

    public decimal PricePerM3 { get; private set; }

    public decimal PricePerOrder { get; private set; }

    [ValidateTuple("FromTo")]
    public bool IsActive { get; private set; }

    public int CurrencyId { get; private set; }

    public decimal MinimumPrice { get; private set; }

    public Guid? CarrierId { get; private set; }

    public Currency.Currency Currency { get; private set; } = null!;
    
    private StorageRoute() {}

    private StorageRoute(
        string from, 
        string to, 
        int distanceM, 
        RouteType routeType, 
        LogisticPricingType pricingModel,
        int deliveryTimeMinutes,
        decimal priceKg,
        decimal pricePerM3,
        decimal pricePerOrder,
        decimal minimumPrice,
        int currencyId,
        Guid? carrierId)
    {
        SetRoute(from, to);
        SetDistanceM(distanceM);
        SetPrices(priceKg, pricePerM3, pricePerOrder);
        SetDeliveryTime(deliveryTimeMinutes);
        SetMinimumPrice(minimumPrice);
        SetCurrencyId(currencyId);
        SetCarrierId(carrierId);
        SetRouteType(routeType);
        SetPricingModel(pricingModel);
    }

    public static StorageRoute Create(
        string from,
        string to,
        int distanceM,
        RouteType routeType,
        LogisticPricingType pricingModel,
        int deliveryTimeMinutes,
        decimal priceKg,
        decimal pricePerM3,
        decimal pricePerOrder,
        decimal minimumPrice,
        int currencyId,
        Guid? carrierId)
    {
        return new StorageRoute(
            from, 
            to, 
            distanceM, 
            routeType, 
            pricingModel, 
            deliveryTimeMinutes, 
            priceKg, 
            pricePerM3,
            pricePerOrder, 
            minimumPrice, 
            currencyId, 
            carrierId);
    }

    private void SetRoute(string from, string to)
    {
        (from, to).Against(x => x.from.Trim() == x.to.Trim(), "storage.route.same.storages");
        FromStorageName = from;
        ToStorageName = to;
    }

    public void SetDistanceM(int distanceM)
    {
        DistanceM = distanceM.AgainstTooSmall(1, "storage.route.distance.min");
    }

    public void SetDeliveryTime(int minutes)
    {
        DeliveryTimeMinutes = minutes.AgainstTooSmall(1, "storage.route.delivery.time.min");
    }

    public void SetPrices(decimal priceKg, decimal pricePerM3, decimal pricePerOrder)
    {
        SetPriceKg(priceKg);
        SetPricePerM3(pricePerM3);
        SetPricePerOrder(pricePerOrder);
    }

    public void SetPriceKg(decimal priceKg)
    {
        PriceKg = priceKg.AgainstTooManyDecimalPlaces(2, "storage.route.price.kg.precision")
            .AgainstTooSmall(0, "storage.route.price.kg.min");
    }

    public void SetPricePerM3(decimal pricePerM3)
    {
        PricePerM3 = pricePerM3.AgainstTooManyDecimalPlaces(2, "storage.route.price.m3.precision")
            .AgainstTooSmall(0, "storage.route.price.m3.min");
    }

    public void SetPricePerOrder(decimal pricePerOrder)
    {
        PricePerOrder = pricePerOrder.AgainstTooManyDecimalPlaces(2, "storage.route.price.order.precision")
            .AgainstTooSmall(0, "storage.route.price.order.min");
    }

    public void SetMinimumPrice(decimal minimumPrice)
    {
        MinimumPrice = minimumPrice
            .AgainstTooManyDecimalPlaces(2, "storage.route.minimum.price.precision")
            .AgainstTooSmall(0, "storage.route.minimum.price.min");
    }

    public void SetCurrencyId(int currencyId)
    {
        CurrencyId = currencyId;
    }
    
    public void SetCarrierId(Guid? carrierId)
    {
        CarrierId = carrierId;
    }

    public void SetRouteType(RouteType routeType)
    {
        RouteType = routeType;
    }

    public void SetPricingModel(LogisticPricingType pricingModel)
    {
        PricingModel = pricingModel;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
    
    public override Guid GetId() => Id;
}