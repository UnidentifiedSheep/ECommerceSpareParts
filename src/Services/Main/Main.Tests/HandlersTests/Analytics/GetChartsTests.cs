using FluentAssertions;
using Main.Modules.Analytics.Application.Charts.SalesProfitOverTime;
using Main.Modules.Analytics.Application.Queries;

namespace Tests.HandlersTests.Analytics;

public class GetChartsTests : ServiceTest
{
	[Fact]
	public async Task GetCharts_ReturnsRegisteredChartWithSchemasAndLocalizedDescriptions()
	{
		var result = await Mediator.Send(new GetChartsQuery(), CancellationToken);
		var chart = result.Charts
			.Should()
			.ContainSingle(item => item.SystemName == SalesProfitOverTimeChartDataSource.DataSourceSystemName)
			.Subject;
		chart.Name.Should().NotBeNullOrWhiteSpace();
		chart.Description.Should().NotBeNullOrWhiteSpace();
		chart.QueryInputSchema.Should().NotBeNull();
		chart.DataPointSchema.Should().NotBeNull();
	}
}
