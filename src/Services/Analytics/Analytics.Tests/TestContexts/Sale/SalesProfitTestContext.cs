using Analytics.Entities;
using Analytics.Integration.Tests.DataBuilders.Sale;
using Analytics.Persistence.Context;
using Tests.Abstractions;

namespace Analytics.Integration.Tests.TestContexts.Sale;

public sealed class SalesProfitTestContext(DContext context) : TestContextBase<DContext>(context)
{
	public DateTime Period { get; } = new(
		2026,
		1,
		15,
		10,
		0,
		0,
		DateTimeKind.Utc);

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

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		SalesFact = BuildFact(
			1,
			1,
			Period,
			OrganizationId,
			BuyerId,
			1.123456789012345m,
			0.123456789012345m,
			2);
		SameDaySalesFact = BuildFact(
			2,
			2,
			Period.AddHours(8),
			OrganizationId,
			BuyerId,
			2.987654321098765m,
			1.111111111111111m,
			3);
		SameMonthSalesFact = BuildFact(
			6,
			6,
			new DateTime(
				2026,
				1,
				20,
				12,
				0,
				0,
				DateTimeKind.Utc),
			Guid.NewGuid(),
			Guid.NewGuid(),
			5m,
			2m,
			1);
		NextMonthSalesFact = BuildFact(
			3,
			3,
			new DateTime(
				2026,
				2,
				10,
				12,
				0,
				0,
				DateTimeKind.Utc),
			OrganizationId,
			Guid.NewGuid(),
			10m,
			4m,
			1);
		NextYearSalesFact = BuildFact(
			4,
			4,
			new DateTime(
				2027,
				3,
				5,
				12,
				0,
				0,
				DateTimeKind.Utc),
			Guid.NewGuid(),
			BuyerId,
			20m,
			5m,
			4);
		DeletedSalesFact = BuildFact(
			5,
			5,
			Period.AddHours(4),
			OrganizationId,
			BuyerId,
			100m,
			1m,
			10);
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
