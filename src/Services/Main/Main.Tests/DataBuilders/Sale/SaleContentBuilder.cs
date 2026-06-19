using Bogus;
using Main.Entities.Sale;
using Test.Common.Abstractions;

namespace Tests.DataBuilders.Sale;

public class SaleContentBuilder(Faker faker) : BuilderBase<SaleContent>(faker)
{
    public int? ProductId { get; private set; }
    
    public decimal? PriceWithDiscount { get; private set; }
    public decimal? PriceWithOutDiscount { get; private set; }
    
    private readonly List<int> _storageContentIds = [];
    public IReadOnlyList<int> StorageContentIds => _storageContentIds;
    
    public int? CurrencyId { get; private set; }
    
    public int? DetailsCount { get; private set; }
    public int? Count { get; private set; }

    public SaleContentBuilder WithProductId(int productId)
    {
        ProductId = productId;
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
            ProductId ?? Faker.Random.Int(1),
            price,
            PriceWithDiscount ?? price,
            GetDetails());
    }

    private List<SaleContentDetail> GetDetails()
    {
        var details = new List<SaleContentDetail>();
        Count ??= Faker.Random.Int(1, 100);
        DetailsCount ??= Faker.Random.Int(1, Math.Min(5, Count.Value));

        int remaining = Count.Value;
        var detailsCount = Math.Min(DetailsCount.Value, Count.Value);
        
        var builder = new SaleContentDetailBuilder(Faker)
            .WithPurchaseDate(Faker.Date.Recent())
            .WithPurchasePrice(Math.Round(Faker.Random.Decimal(1), 2))
            .WithStorageContentIds(_storageContentIds)
            .WithCurrencyId(CurrencyId ?? Faker.Random.Int(1));
        
        for (var i = 0; i < detailsCount; i++)
        {
            var remainingDetails = detailsCount - i;
            var toTake = i == detailsCount - 1
                ? remaining
                : Faker.Random.Int(1, remaining - remainingDetails + 1);

            remaining -= toTake;
            
            details.Add(builder.WithCount(toTake).Build());
        }

        return details;
    }
}
