using FluentAssertions;
using Main.Application.Analytics.Charts.SalesProfitOverTime;
using Main.Application.Analytics.Queries;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Analytics;

public class GetChartsTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task GetCharts_ReturnsRegisteredChartWithSchemasAndLocalizedDescriptions()
	{
		var result = await Mediator.Send(new GetChartsQuery(), CancellationToken);
		var chart = result.Charts.Should().ContainSingle(item => item.SystemName == SalesProfitOverTimeChartDataSource.DataSourceSystemName).Subject;
		chart.Name.Should().NotBeNullOrWhiteSpace();
		chart.Description.Should().NotBeNullOrWhiteSpace();
		chart.QueryInputSchema.Should().NotBeNull();
		chart.DataPointSchema.Should().NotBeNull();
	}
}
