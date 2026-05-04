using Bogus;
using Main.Entities.Storage;
using Main.Enums;
using Test.Common.Abstractions;

namespace Tests.DataBuilders.Storage;

public class StorageRouteBuilder(Faker faker) : BuilderBase<StorageRoute>(faker)
{
    public string? StorageFrom { get; private set; }
    public string? StorageTo { get; private set; }

    public int? DistanceM { get; private set; }
    public RouteType? RouteType { get; private set; }
    public LogisticPricingType? PricingModel { get; private set; }
    public int? DeliveryTimeMinutes { get; private set; }

    public decimal? PriceKg { get; private set; }
    public decimal? PricePerM3 { get; private set; }
    public decimal? PricePerOrder { get; private set; }
    public decimal? MinimumPrice { get; private set; }

    public int? CurrencyId { get; private set; }
    public Guid? CarrierId { get; private set; }

    public bool? IsActive { get; private set; }

    public StorageRouteBuilder WithFrom(string from)
    {
        StorageFrom = from;
        return this;
    }

    public StorageRouteBuilder WithTo(string to)
    {
        StorageTo = to;
        return this;
    }

    public StorageRouteBuilder WithDistance(int distanceM)
    {
        DistanceM = distanceM;
        return this;
    }

    public StorageRouteBuilder WithRouteType(RouteType routeType)
    {
        RouteType = routeType;
        return this;
    }

    public StorageRouteBuilder WithPricingModel(LogisticPricingType pricingModel)
    {
        PricingModel = pricingModel;
        return this;
    }

    public StorageRouteBuilder WithDeliveryTime(int minutes)
    {
        DeliveryTimeMinutes = minutes;
        return this;
    }

    public StorageRouteBuilder WithPriceKg(decimal priceKg)
    {
        PriceKg = priceKg;
        return this;
    }

    public StorageRouteBuilder WithPricePerM3(decimal pricePerM3)
    {
        PricePerM3 = pricePerM3;
        return this;
    }

    public StorageRouteBuilder WithPricePerOrder(decimal pricePerOrder)
    {
        PricePerOrder = pricePerOrder;
        return this;
    }

    public StorageRouteBuilder WithMinimumPrice(decimal minimumPrice)
    {
        MinimumPrice = minimumPrice;
        return this;
    }

    public StorageRouteBuilder WithCurrencyId(int currencyId)
    {
        CurrencyId = currencyId;
        return this;
    }

    public StorageRouteBuilder WithCarrierId(Guid? carrierId)
    {
        CarrierId = carrierId;
        return this;
    }

    public StorageRouteBuilder Active()
    {
        IsActive = true;
        return this;
    }

    public StorageRouteBuilder Inactive()
    {
        IsActive = false;
        return this;
    }

    public override StorageRoute Build()
    {
        var route = StorageRoute.Create(
            StorageFrom ?? Faker.Address.City(),
            StorageTo ?? Faker.Address.City(),
            DistanceM ?? Faker.Random.Int(1, 100_000),
            RouteType ?? Faker.PickRandom<RouteType>(),
            PricingModel ?? Faker.PickRandom<LogisticPricingType>(),
            DeliveryTimeMinutes ?? Faker.Random.Int(1, 10_000),
            PriceKg ?? Math.Round(Faker.Random.Decimal(0, 100), 2),
            PricePerM3 ?? Math.Round(Faker.Random.Decimal(0, 100), 2),
            PricePerOrder ?? Math.Round(Faker.Random.Decimal(0, 100), 2),
            MinimumPrice ?? Math.Round(Faker.Random.Decimal(0, 100), 2),
            CurrencyId ?? Faker.Random.Int(1, 10),
            CarrierId
        );

        if (!IsActive.HasValue) return route;
        
        if (IsActive.Value)
            route.Activate();
        else
            route.Deactivate();

        return route;
    }
}