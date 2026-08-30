using Analytics.Entities;
using Analytics.Integration.Tests.DataBuilders.Sale;
using Analytics.Persistence.Context;
using Tests.Abstractions;

namespace Analytics.Integration.Tests.TestContexts.Sale;

public sealed class SalesProfitTestContext(DContext context)
    : TestContextBase<DContext>(context)
{
    public DateTime Period { get; private set; }
    public SalesFact SalesFact { get; private set; } = null!;
    public SalesFact DeletedSalesFact { get; private set; } = null!;

    public override async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        Period = DateTime.UtcNow;
        var factId = Guid.NewGuid();
        const int contentId = 1;
        var detail = new SaleContentDetailBuilder(Faker)
            .WithId(1)
            .WithSaleContentId(contentId)
            .WithCurrencyId(1)
            .WithBuyPrice(0.123456789012345m)
            .WithBuyPriceInBaseCurrency(0.123456789012345m)
            .WithCount(1)
            .WithPurchaseDate(Period)
            .Build();
        var content = new SaleContentBuilder(Faker)
            .WithId(contentId)
            .WithSaleId(factId)
            .WithProductId(1)
            .WithPrice(1.123456789012345m)
            .WithPriceInBaseCurrency(1.123456789012345m)
            .WithCount(1)
            .WithDetails([detail])
            .Build();

        SalesFact = new SalesFactBuilder(Faker)
            .WithId(factId)
            .WithCurrencyId(1)
            .WithBaseCurrencyId(1)
            .WithCreatedAt(Period)
            .WithProcessedAt(Period)
            .WithContents([content])
            .Build();
        DeletedSalesFact = new SalesFactBuilder(Faker)
            .WithProcessedAt(Period)
            .Deleted()
            .Build();

        await DbContext.AddRangeAsync(
            SalesFact,
            DeletedSalesFact);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
