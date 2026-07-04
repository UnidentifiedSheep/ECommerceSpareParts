using Bogus;
using Main.Entities.Sale;
using Tests.Abstractions;

namespace Tests.DataBuilders.Sale;

public class SaleContentDetailBuilder(Faker faker) : BuilderBase<SaleContentDetail>(faker)
{
    private readonly List<int> _storageContentIds = [];
    public IReadOnlyList<int> StorageContentIds => _storageContentIds;

    public int? CurrencyId { get; private set; }

    public int? Count { get; private set; }
    public decimal? PurchasePrice { get; private set; }
    public DateTime? PurchaseDate { get; private set; }

    public SaleContentDetailBuilder WithStorageContentIds(IEnumerable<int> storageContentIds)
    {
        _storageContentIds.Clear();
        _storageContentIds.AddRange(storageContentIds);
        return this;
    }

    public SaleContentDetailBuilder WithCurrencyId(int currencyId)
    {
        CurrencyId = currencyId;
        return this;
    }

    public SaleContentDetailBuilder WithCount(int count)
    {
        Count = count;
        return this;
    }

    public SaleContentDetailBuilder WithPurchasePrice(decimal price)
    {
        PurchasePrice = price;
        return this;
    }

    public SaleContentDetailBuilder WithPurchaseDate(DateTime purchaseDate)
    {
        PurchaseDate = purchaseDate;
        return this;
    }

    public override SaleContentDetail Build()
    {
        return SaleContentDetail.Create(
            StorageContentIds.Count > 0 ? Faker.PickRandom<int>(StorageContentIds) : Faker.Random.Int(1),
            CurrencyId ?? Faker.Random.Int(1),
            PurchasePrice ?? Math.Round(Faker.Random.Decimal(1), 2),
            Count ?? Faker.Random.Int(1),
            PurchaseDate ?? Faker.Date.Past());
    }
}