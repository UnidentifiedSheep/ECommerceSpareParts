using Analytics.Entities;
using Analytics.Integration.Tests.DataBuilders.Sale;
using Analytics.Persistence.Context;
using Tests.Abstractions;

namespace Analytics.Integration.Tests.TestContexts.Sale;

public sealed class SalesProfitTestContext(DContext context)
    : TestContextBase<DContext>(context)
{
    public DateTime Period { get; } = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
    public Guid OrganizationId { get; } = Guid.NewGuid();
    public Guid BuyerId { get; } = Guid.NewGuid();
    public SalesFact SalesFact { get; private set; } = null!;
    public SalesFact SameDaySalesFact { get; private set; } = null!;
    public SalesFact SameMonthSalesFact { get; private set; } = null!;
    public SalesFact NextMonthSalesFact { get; private set; } = null!;
    public SalesFact NextYearSalesFact { get; private set; } = null!;
    public SalesFact DeletedSalesFact { get; private set; } = null!;

    public IReadOnlyList<SalesFact> ActiveSalesFacts =>
    [
        SalesFact,
        SameDaySalesFact,
        SameMonthSalesFact,
        NextMonthSalesFact,
        NextYearSalesFact
    ];

    public override async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        SalesFact = BuildFact(
            contentId: 1,
            detailId: 1,
            createdAt: Period,
            organizationId: OrganizationId,
            buyerId: BuyerId,
            price: 1.123456789012345m,
            buyPrice: 0.123456789012345m,
            count: 2);
        SameDaySalesFact = BuildFact(
            contentId: 2,
            detailId: 2,
            createdAt: Period.AddHours(8),
            organizationId: OrganizationId,
            buyerId: BuyerId,
            price: 2.987654321098765m,
            buyPrice: 1.111111111111111m,
            count: 3);
        SameMonthSalesFact = BuildFact(
            contentId: 6,
            detailId: 6,
            createdAt: new DateTime(2026, 1, 20, 12, 0, 0, DateTimeKind.Utc),
            organizationId: Guid.NewGuid(),
            buyerId: Guid.NewGuid(),
            price: 5m,
            buyPrice: 2m,
            count: 1);
        NextMonthSalesFact = BuildFact(
            contentId: 3,
            detailId: 3,
            createdAt: new DateTime(2026, 2, 10, 12, 0, 0, DateTimeKind.Utc),
            organizationId: OrganizationId,
            buyerId: Guid.NewGuid(),
            price: 10m,
            buyPrice: 4m,
            count: 1);
        NextYearSalesFact = BuildFact(
            contentId: 4,
            detailId: 4,
            createdAt: new DateTime(2027, 3, 5, 12, 0, 0, DateTimeKind.Utc),
            organizationId: Guid.NewGuid(),
            buyerId: BuyerId,
            price: 20m,
            buyPrice: 5m,
            count: 4);
        DeletedSalesFact = BuildFact(
            contentId: 5,
            detailId: 5,
            createdAt: Period.AddHours(4),
            organizationId: OrganizationId,
            buyerId: BuyerId,
            price: 100m,
            buyPrice: 1m,
            count: 10);
        DeletedSalesFact.MarkDeleted(Period.AddDays(1));

        await DbContext.AddRangeAsync(
            SalesFact,
            SameDaySalesFact,
            SameMonthSalesFact,
            NextMonthSalesFact,
            NextYearSalesFact,
            DeletedSalesFact);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    private SalesFact BuildFact(
        int contentId,
        int detailId,
        DateTime createdAt,
        Guid organizationId,
        Guid buyerId,
        decimal price,
        decimal buyPrice,
        int count)
    {
        var factId = Guid.NewGuid();
        var detail = new SaleContentDetailBuilder(Faker)
            .WithId(detailId)
            .WithSaleContentId(contentId)
            .WithCurrencyId(1)
            .WithBuyPrice(buyPrice)
            .WithBuyPriceInBaseCurrency(buyPrice)
            .WithCount(count)
            .WithPurchaseDate(createdAt)
            .Build();
        var content = new SaleContentBuilder(Faker)
            .WithId(contentId)
            .WithSaleId(factId)
            .WithProductId(contentId)
            .WithPrice(price)
            .WithPriceInBaseCurrency(price)
            .WithCount(count)
            .WithDetails([detail])
            .Build();

        return new SalesFactBuilder(Faker)
            .WithId(factId)
            .WithCurrencyId(1)
            .WithBaseCurrencyId(1)
            .WithOrganizationId(organizationId)
            .WithBuyerId(buyerId)
            .WithCreatedAt(createdAt)
            .WithProcessedAt(createdAt)
            .WithContents([content])
            .Build();
    }
}
