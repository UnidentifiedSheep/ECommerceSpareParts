using Bogus;
using Main.Entities.Sale;
using Test.Common.Abstractions;

namespace Tests.DataBuilders.Sale;

public class SaleContentBuilder(Faker faker) : BuilderBase<SaleContent>(faker)
{
    private readonly List<int> _productIds = [];
    public IReadOnlyList<int> ProductIds => _productIds;
    
    public decimal? PriceWithDiscount { get; private set; }
    public decimal? PriceWithOutDiscount { get; private set; }
    
    private List<int> _storageContentIds = [];
    public IReadOnlyList<int> StorageContentIds => _storageContentIds;
    
    public int? CurrencyId { get; private set; }
    
    public int? DetailsCount { get; private set; }
    public int? Count { get; private set; }
    
    public SaleContentBuilder WithProductId(int productId)
        => WithProductIds([productId]);

    public SaleContentBuilder WithProductIds(IEnumerable<int> productIds)
    {
        _productIds.Clear();
        _productIds.AddRange(productIds);
        return this;
    }

    public SaleContentBuilder WithDetailsCount(int detailsCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(detailsCount);
        DetailsCount = detailsCount;
        return this;
    }

    public SaleContentBuilder WithCount(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        Count = count;
        return this;
    }

    public SaleContentBuilder WithStorageContentIds(IEnumerable<int> storageContentIds)
    {
        _storageContentIds.Clear();
        _storageContentIds.AddRange(storageContentIds);
        return this;
    }

    public SaleContentBuilder WithCurrencyId(int currencyId)
    {
        CurrencyId = currencyId;
        return this;
    }
    
    public override SaleContent Build()
    {
        var price = PriceWithOutDiscount ?? Math.Round(Faker.Random.Decimal(1), 2);
        return SaleContent.Create(
            ProductIds.Count > 0 ? Faker.PickRandom<int>(ProductIds) : Faker.Random.Int(1),
            price,
            PriceWithDiscount ?? price,
            GetDetails());
    }

    private List<SaleContentDetail> GetDetails()
    {
        var details = new List<SaleContentDetail>();
        Count ??= Faker.Random.Int(1, 100);
        DetailsCount ??= Faker.Random.Int(1, 5);

        int remaining = Count.Value;
        int currDetailsCount = 0;
        int chunk = remaining / DetailsCount.Value;
        
        var builder = new SaleContentDetailBuilder(Faker)
            .WithPurchaseDate(Faker.Date.Recent())
            .WithPurchasePrice(Math.Round(Faker.Random.Decimal(1), 2))
            .WithStorageContentIds(_storageContentIds)
            .WithCurrencyId(CurrencyId ?? Faker.Random.Int(1));
        
        while (remaining > 0 && currDetailsCount++ < DetailsCount)
        {
            int toTake = Math.Min(remaining, chunk);
            remaining -= toTake;
            
            details.Add(builder.WithCount(toTake).Build());
        }

        return details;
    }
}