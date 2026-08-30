using Analytics.Application.Models;
using Analytics.Application.NamedObjects.ChartDataSources;
using Analytics.Application.NamedObjects.ChartDataSources.SalesProfitOverTime;
using Analytics.Entities;
using Analytics.Entities.Enums;
using Analytics.Integration.Tests.TestContexts.Sale;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;

namespace Analytics.Integration.Tests.NamedObjects.ChartDataSources;

public sealed class SalesProfitOverTimeChartDataSourceTests : IntegrationTest
{
    public SalesProfitOverTimeChartDataSourceTests(
        CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<SalesProfitTestContext>();
    }

    private SalesProfitTestContext TestContext => GetContext<SalesProfitTestContext>();

    [Fact]
    public async Task QueryAsync_AggregatesMetricsRoundsDecimalsAndExcludesDeletedFacts()
    {
        var includedFacts = new[]
        {
            TestContext.SalesFact,
            TestContext.SameDaySalesFact
        };

        var result = await QueryAsync(
            StartOfDay(TestContext.Period),
            EndOfDay(TestContext.Period));

        result.DataPoints.Should().ContainSingle();
        var dataPoint = result.DataPoints.Single();
        dataPoint.PeriodStart.Should().Be(StartOfDay(TestContext.Period));
        dataPoint.Revenue.Should().Be(RoundedSum(
            includedFacts,
            fact => fact.RevenueInBaseCurrency));
        dataPoint.Cost.Should().Be(RoundedSum(
            includedFacts,
            fact => fact.CostInBaseCurrency));
        dataPoint.GrossProfit.Should().Be(RoundedSum(
            includedFacts,
            fact => fact.GrossProfitInBaseCurrency));
        dataPoint.SalesCount.Should().Be(includedFacts.Length);
        dataPoint.ProductsCount.Should().Be(includedFacts.Sum(x => x.ProductsCount));
        dataPoint.Margin.Should().Be(dataPoint.GrossProfit / dataPoint.Revenue);
    }

    [Theory]
    [InlineData(TimeGranularity.Day, 4)]
    [InlineData(TimeGranularity.Month, 3)]
    [InlineData(TimeGranularity.Year, 2)]
    public async Task QueryAsync_GroupsFactsByRequestedGranularity(
        TimeGranularity granularity,
        int expectedDataPoints)
    {
        var result = await QueryAsync(
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2027, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            granularity);

        result.DataPoints.Should().HaveCount(expectedDataPoints);
        result.DataPoints.Sum(x => x.SalesCount)
            .Should().Be(TestContext.ActiveSalesFacts.Count);
    }

    [Fact]
    public async Task QueryAsync_AppliesOrganizationAndBuyerFiltersTogether()
    {
        var result = await QueryAsync(new SalesProfitChartQuery
        {
            StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            Granularity = TimeGranularity.Day,
            OrganizationId = TestContext.OrganizationId,
            BuyerId = TestContext.BuyerId
        });

        result.DataPoints.Should().ContainSingle();
        result.DataPoints.Single().SalesCount.Should().Be(2);
    }

    [Fact]
    public async Task QueryAsync_IncludesFactsOnDateRangeBoundaries()
    {
        var result = await QueryAsync(
            TestContext.SalesFact.CreatedAt,
            TestContext.SalesFact.CreatedAt);

        result.DataPoints.Should().ContainSingle();
        result.DataPoints.Single().SalesCount.Should().Be(1);
    }

    [Fact]
    public async Task QueryAsync_ReturnsEmptyResultWhenNoFactsMatch()
    {
        var result = await QueryAsync(
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc));

        result.DataPoints.Should().BeEmpty();
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task QueryAsync_AppliesCursorPagination()
    {
        var firstPage = await QueryAsync(new SalesProfitChartQuery
        {
            StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            Granularity = TimeGranularity.Day,
            Size = 1
        });

        firstPage.DataPoints.Should().ContainSingle();
        firstPage.DataPoints.Single().PeriodStart.Should().Be(StartOfDay(TestContext.Period));
        firstPage.NextCursor.Should().NotBeNull();

        var secondPage = await QueryAsync(new SalesProfitChartQuery
        {
            StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            Granularity = TimeGranularity.Day,
            Cursor = firstPage.DataPoints.Single().PeriodStart,
            Size = 1
        });

        secondPage.DataPoints.Should().ContainSingle();
        secondPage.DataPoints.Single().PeriodStart.Should().Be(
            StartOfDay(TestContext.SameMonthSalesFact.CreatedAt));
    }

    private Task<ChartDataResult<SalesProfitDataPoint>> QueryAsync(
        DateTime startDate,
        DateTime endDate,
        TimeGranularity granularity = TimeGranularity.Day)
    {
        return QueryAsync(new SalesProfitChartQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            Granularity = granularity
        });
    }

    private Task<ChartDataResult<SalesProfitDataPoint>> QueryAsync(
        SalesProfitChartQuery query)
    {
        var dataSource = Scope.ServiceProvider
            .GetServices<ChartDataSourceNamedObject>()
            .OfType<SalesProfitOverTimeChartDataSource>()
            .Single();

        return dataSource.QueryAsync(query, CancellationToken.None);
    }

    private static decimal RoundedSum(
        IEnumerable<SalesFact> facts,
        Func<SalesFact, decimal> selector)
    {
        return Math.Round(facts.Sum(selector), 12);
    }

    private static DateTime StartOfDay(DateTime value)
    {
        return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    private static DateTime EndOfDay(DateTime value)
    {
        return StartOfDay(value).AddDays(1).AddTicks(-1);
    }
}
