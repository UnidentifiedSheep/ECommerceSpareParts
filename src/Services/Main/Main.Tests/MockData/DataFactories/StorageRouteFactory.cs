using Bogus;
using Main.Entities;
using Main.Enums;

namespace Tests.MockData.DataFactories;

public static class StorageRouteFactory
{
    private readonly static Faker<StorageRoute> Faker = new Faker<StorageRoute>()
        .RuleFor(x => x.FromStorageName, f => f.Lorem.Word())
        .RuleFor(x => x.ToStorageName, f => f.Lorem.Word())
        .RuleFor(x => x.CarrierId, f => f.Random.Guid())
        .RuleFor(x => x.CurrencyId, f => f.Random.Int(1))
        .RuleFor(x => x.DeliveryTimeMinutes, f => f.Random.Int(300, 600000))
        .RuleFor(x => x.DistanceM, f => f.Random.Int(10, 100000))
        .RuleFor(x => x.IsActive, f => f.Random.Bool())
        .RuleFor(x => x.MinimumPrice, f => f.Random.Decimal(10, 100000))
        .RuleFor(x => x.PriceKg, f => f.Random.Decimal(10, 100000))
        .RuleFor(x => x.PricePerM3, f => f.Random.Decimal(10, 100000))
        .RuleFor(x => x.PricePerOrder, f => f.Random.Decimal(10, 100000))
        .RuleFor(x => x.PricingModel, f => f.PickRandom<LogisticPricingType>())
        .RuleFor(x => x.RouteType, f => f.PickRandom<RouteType>());
    
    public static List<StorageRoute> Create(int count) =>  Faker.Generate(count);

    public static List<StorageRoute> Create(int count, IEnumerable<string> storageNames, IEnumerable<Guid> userIds,
        IEnumerable<int> currencyIds)
    {
        var clone = Faker.Clone()
            .RuleFor(x => x.FromStorageName, f => f.PickRandom(storageNames))
            .RuleFor(x => x.CarrierId, f => f.PickRandom(userIds))
            .RuleFor(x => x.CurrencyId, f => f.PickRandom(currencyIds))
            .FinishWith((f, d) =>
            {
                var exceptStorageName = storageNames.ToList();
                exceptStorageName.Remove(d.FromStorageName);
                d.ToStorageName = f.PickRandom(exceptStorageName);
            });
        
        return clone.Generate(count);
    }
}