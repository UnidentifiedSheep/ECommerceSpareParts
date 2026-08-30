using Analytics.Application.NamedObjects.ChartDataSources;
using Analytics.Application.NamedObjects.ChartDataSources.SalesProfitOverTime;
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
    public async Task QueryAsync_RoundsAggregatesAndExcludesDeletedFacts()
    {
        var dataSource = Scope.ServiceProvider
            .GetServices<ChartDataSourceNamedObject>()
            .OfType<SalesProfitOverTimeChartDataSource>()
            .Single();
        var result = await dataSource.QueryAsync(
            new SalesProfitChartQuery
            {
                StartDate = TestContext.Period.AddDays(-1),
                EndDate = TestContext.Period.AddDays(1),
                Granularity = TimeGranularity.Day
            },
            CancellationToken.None);

        result.DataPoints.Should().ContainSingle();
        result.DataPoints[0].Revenue.Should().Be(
            Math.Round(TestContext.SalesFact.RevenueInBaseCurrency, 12));
        result.DataPoints[0].Cost.Should().Be(
            Math.Round(TestContext.SalesFact.CostInBaseCurrency, 12));
        result.DataPoints[0].GrossProfit.Should().Be(
            Math.Round(TestContext.SalesFact.GrossProfitInBaseCurrency, 12));
        result.DataPoints[0].SalesCount.Should().Be(1);
    }
}
