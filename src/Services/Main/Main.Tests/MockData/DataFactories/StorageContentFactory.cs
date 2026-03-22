using Bogus;
using Main.Entities;

namespace Tests.MockData.DataFactories;

public static class StorageContentFactory
{
    private static Faker<StorageContent> _faker = new Faker<StorageContent>()
        .RuleFor(x => x.ArticleId, f => f.UniqueIndex)
        .RuleFor(x => x.CurrencyId, f => f.UniqueIndex)
        .RuleFor(x => x.BuyPrice, f => decimal.Parse(f.Commerce.Price()))
        .RuleFor(x => x.BuyPriceInUsd, f => decimal.Parse(f.Commerce.Price()))
        .RuleFor(x => x.Count, f => f.Random.Int(10, 100))
        .RuleFor(x => x.StorageName, f => f.Lorem.Word());
    
    public static List<StorageContent> Create(int count) => _faker.Generate(count);

    public static List<StorageContent> Create(int count, IEnumerable<int> currencyIds, IEnumerable<int> articleIds,
        IEnumerable<string> storageNames)
    {
        var clone = _faker.Clone()
            .RuleFor(x => x.ArticleId, f => f.PickRandom(articleIds))
            .RuleFor(x => x.StorageName, f => f.PickRandom(storageNames))
            .RuleFor(x => x.CurrencyId, f => f.PickRandom(currencyIds));
        
        return clone.Generate(count);
    }
}