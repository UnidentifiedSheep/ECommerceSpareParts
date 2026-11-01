using Exceptions;
using FluentAssertions;
using Main.Application.Analytics.Charts.SalesProfitOverTime;
using Main.Application.Analytics.Queries;
using Main.Entities.Exceptions.Analytics;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Analytics;

public class QueryChartDataSourceTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task QueryChart_RegisteredChart_ExecutesThroughRegistry()
	{
		var result = await Mediator.Send(new QueryChartDataSourceQuery(
			SalesProfitOverTimeChartDataSource.DataSourceSystemName,
			"""{"startDate":"2026-01-01T00:00:00Z","endDate":"2026-12-31T00:00:00Z","granularity":"Day"}"""), CancellationToken);
		result.DataPoints.Should().BeEmpty();
	}

	[Fact]
	public async Task QueryChart_UnknownName_ThrowsNotFound() =>
		await Assert.ThrowsAsync<ChartDataSourceNotFoundException>(() => Mediator.Send(
			new QueryChartDataSourceQuery("missing", "{}"), CancellationToken));

	[Fact]
	public async Task QueryChart_InvalidInput_ThrowsInvalidInput() =>
		await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(
			new QueryChartDataSourceQuery(SalesProfitOverTimeChartDataSource.DataSourceSystemName, "{"), CancellationToken));
}
