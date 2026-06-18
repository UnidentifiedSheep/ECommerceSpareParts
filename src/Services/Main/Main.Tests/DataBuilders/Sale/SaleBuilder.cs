using Bogus;
using Main.Entities.Sale;
using Test.Common.Abstractions;

namespace Tests.DataBuilders.Sale;

public class SaleBuilder(Faker faker) : BuilderBase<Main.Entities.Sale.Sale>(faker)
{
    public Guid? BuyerId { get; private set; }
    public Guid? TransactionId { get; private set; }
    public int? CurrencyId { get; private set; }
    public string? StorageName { get; private set; }
    public DateTime? SaleDate { get; private set; }
    public bool CompleteSale { get; private set; }

    private readonly List<SaleContent> _contents = [];
    public IReadOnlyList<SaleContent> Contents => _contents;

    public SaleBuilder WithBuyerId(Guid id)
    {
        BuyerId = id;
        return this;
    }

    public SaleBuilder WithTransactionId(Guid id)
    {
        TransactionId = id;
        return this;
    }

    public SaleBuilder WithCurrencyId(int id)
    {
        CurrencyId = id;
        return this;
    }

    public SaleBuilder WithStorageName(string name)
    {
        StorageName = name;
        return this;
    }

    public SaleBuilder WithSaleDate(DateTime date)
    {
        SaleDate = date;
        return this;
    }

    public SaleBuilder WithContents(IEnumerable<SaleContent> contents)
    {
        _contents.Clear();
        _contents.AddRange(contents);
        return this;
    }

    public SaleBuilder Completed()
    {
        CompleteSale = true;
        return this;
    }
    
    public override Main.Entities.Sale.Sale Build()
    {
        var sale = Main.Entities.Sale.Sale.Create(
            BuyerId ?? Guid.NewGuid(),
            TransactionId ?? Guid.NewGuid(),
            CurrencyId ?? Faker.Random.Int(1),
            StorageName ?? Faker.Lorem.Letter(8),
            SaleDate ?? DateTime.Now);

        foreach (var content in _contents)
            sale.AddContent(content);

        if (CompleteSale)
            sale.Complete();
        
        return sale;
    }
}
