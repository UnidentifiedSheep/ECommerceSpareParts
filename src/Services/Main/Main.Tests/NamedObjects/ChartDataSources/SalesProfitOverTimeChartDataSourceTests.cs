using Main.Entities.Sale;
using Tests.DataBuilders.Sale;
using Tests.TestContexts.Currency;
using FluentAssertions;
using Main.Application.Analytics.Charts;
using Main.Application.Analytics.Charts.SalesProfitOverTime;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Sale;

namespace Tests.NamedObjects.ChartDataSources;

public class SalesProfitOverTimeChartDataSourceTests : IntegrationTest
{
	public SalesProfitOverTimeChartDataSourceTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<SaleTestContext>();
	}

	private SalesProfitOverTimeChartDataSource Source => Scope.ServiceProvider
		.GetServices<ChartDataSourceNamedObject>()
		.OfType<SalesProfitOverTimeChartDataSource>()
		.Single();

	[Theory]
	[InlineData(TimeGranularity.Day)]
	[InlineData(TimeGranularity.Month)]
	[InlineData(TimeGranularity.Year)]
	public async Task Query_AggregatesStoredRevenueAndCost(TimeGranularity granularity)
	{
		var sale = GetContext<SaleTestContext>().Sale;
		sale.SetDateTime(new DateTime(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc));
		var content = sale.Contents.Single();
		var analytics = await Context.SaleContentAnalytics.SingleAsync(item => item.SaleContentId == content.Id, CancellationToken);
		analytics.Update(120m * content.Count, 90m * content.Count, 40m * content.Count);
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Source.QueryAsync(Input() with { Granularity = granularity }, CancellationToken);

		var point = result.DataPoints.Should().ContainSingle().Subject;
		point.Revenue.Should().Be(90m * content.Count);
		point.Cost.Should().Be(40m * content.Count);
		point.GrossProfit.Should().Be(50m * content.Count);
		point.SalesCount.Should().Be(1);
		point.ProductsCount.Should().Be(content.Count);
		point.PeriodStart.Should().Be(new DateTime(2026, granularity == TimeGranularity.Year ? 1 : 5,
			granularity == TimeGranularity.Day ? 15 : 1, 0, 0, 0, DateTimeKind.Utc));
	}

	[Fact]
	public async Task Query_ExcludesDeletedSalesAndAppliesBuyerAndOrganizationFilters()
	{
		var sale = GetContext<SaleTestContext>().Sale;
		sale.SetDateTime(Input().StartDate);
		await Context.SaveChangesAsync(CancellationToken);
		(await Source.QueryAsync(Input() with { OrganizationId = Guid.NewGuid() }, CancellationToken)).DataPoints.Should().BeEmpty();
		(await Source.QueryAsync(Input() with { BuyerId = Guid.NewGuid() }, CancellationToken)).DataPoints.Should().BeEmpty();
		(await Source.QueryAsync(Input() with { OrganizationId = sale.OrganizationId, BuyerId = sale.UserId }, CancellationToken))
			.DataPoints.Should().ContainSingle();
		sale.Delete();
		await Context.SaveChangesAsync(CancellationToken);
		(await Source.QueryAsync(Input(), CancellationToken)).DataPoints.Should().BeEmpty();
	}

	[Fact]
	public async Task Query_MissingSnapshot_RejectsIncompleteHistory()
	{
		var sale = GetContext<SaleTestContext>().Sale;
		sale.SetDateTime(Input().StartDate);
		var content = sale.Contents.Single();
		await Context.SaleContentAnalytics.Where(item => item.SaleContentId == content.Id).ExecuteDeleteAsync(CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		await Assert.ThrowsAsync<InvalidOperationException>(() => Source.QueryAsync(Input(), CancellationToken));
	}

	[Fact]
	public async Task Query_SeeksAcrossPeriodsAndIncludesExactDateBoundaries()
	{
		var context = GetContext<SaleTestContext>();
		var firstDate = Input().StartDate;
		context.Sale.SetDateTime(firstDate);
		var detail = new SaleContentDetailBuilder(Faker).WithCount(1).WithCurrencyId(context.Sale.CurrencyId)
			.WithStorageContentIds([context.StorageContent.Id]).WithPurchasePrice(40m).Build();
		var content = new SaleContentBuilder(Faker).WithProductId(context.Product.Id).WithPrices(120m, 90m)
			.WithDetails([detail]).Build();
		var second = new SaleBuilder(Faker).WithUserId(context.Buyer.Id).WithOrganizationId(context.Buyer.Id)
			.WithCurrencyId(context.Sale.CurrencyId).WithStorageCode(context.Sale.StorageCode)
			.WithTransactionId(context.Transaction.Id).WithSaleDate(firstDate.AddDays(1))
			.WithContents([content]).Completed().Build();
		await Context.AddAsync(second, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		await Context.AddAsync(SaleContentAnalytics.Create(content.Id, 120m, 90m, 40m), CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		var input = Input() with { EndDate = firstDate.AddDays(1), Size = 1 };
		var firstPage = await Source.QueryAsync(input, CancellationToken);
		firstPage.DataPoints.Should().ContainSingle().Which.PeriodStart.Should().Be(firstDate);
		firstPage.NextCursor.Should().NotBeNull();
		var secondPage = await Source.QueryAsync(input with { Cursor = firstPage.DataPoints.Single().PeriodStart }, CancellationToken);
		secondPage.DataPoints.Should().ContainSingle().Which.PeriodStart.Should().Be(firstDate.AddDays(1));
		(await Source.QueryAsync(input with { Cursor = secondPage.DataPoints.Single().PeriodStart }, CancellationToken)).DataPoints.Should().BeEmpty();
	}

	[Fact]
	public async Task Query_ChangedCurrentRates_DoesNotRevalueStoredAnalytics()
	{
		var sale = GetContext<SaleTestContext>().Sale;
		sale.SetDateTime(Input().StartDate);
		await Context.SaveChangesAsync(CancellationToken);
		var before = await Source.QueryAsync(Input(), CancellationToken);
		foreach (var rate in GetContext<CurrencyRatesTestContext>().Rates)
			await Context.AddAsync(Main.Entities.Currency.CurrencyRate.Create(rate.FromCurrencyId, rate.ToCurrencyId, rate.Rate * 2), CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		var after = await Source.QueryAsync(Input(), CancellationToken);
		after.DataPoints.Should().BeEquivalentTo(before.DataPoints);
	}

	private static SalesProfitChartQuery Input() => new()
	{
		StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
		EndDate = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc)
	};
}
