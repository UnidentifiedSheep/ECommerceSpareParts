using Bogus;
using Main.Entities.Product;
using Main.Entities.Storage;
using Test.Common.Abstractions;

namespace Tests.DataBuilders.Storage;

public class StorageContentBuilder(Faker faker) : BuilderBase<StorageContent>(faker)
{
    public string? StorageName { get; private set; }
    public int? CurrencyId { get; private set; }
    public int? Count { get; private set; }
    public decimal? BuyPrice { get; private set; }
    public DateTime? PurchaseDate { get; private set; }
    
    private readonly HashSet<int> _productIds = [];
    public IReadOnlySet<int> ProductIds => _productIds;

    public StorageContentBuilder WithStorageName(string storageName)
    {
        StorageName = storageName;
        return this;
    }

    public StorageContentBuilder WithCurrencyId(int currencyId)
    {
        CurrencyId = currencyId;
        return this;
    }

    public StorageContentBuilder WithCount(int count)
    {
        Count = count;
        return this;
    }

    public StorageContentBuilder WithBuyPrice(decimal buyPrice)
    {
        BuyPrice = buyPrice;
        return this;
    }

    public StorageContentBuilder WithPurchaseDate(DateTime purchaseDate)
    {
        PurchaseDate = purchaseDate;
        return this;
    }

    public StorageContentBuilder WithProductIds(params int[] ids)
    {
        _productIds.UnionWith(ids);
        return this;
    }
    
    public StorageContentBuilder WithProducts(IEnumerable<Product> products)
    {
        _productIds.UnionWith(products.Select(x => x.Id));
        return this;
    }
    
    public override StorageContent Build()
    {
        return StorageContent.Create(
            StorageName ?? Faker.Lorem.Word(),
            _productIds.Count > 0 ? Faker.PickRandom<int>(_productIds) : Faker.Random.Int(1),
            Count ?? Faker.Random.Int(1),
            BuyPrice ?? Math.Round(Faker.Random.Decimal(1, 1000), 2),
            CurrencyId ?? Faker.Random.Int(1),
            BuyPrice ?? Math.Round(Faker.Random.Decimal(1, 1000), 2),
            CurrencyId ?? Faker.Random.Int(1),
            PurchaseDate ?? Faker.Date.Future());
    }
}