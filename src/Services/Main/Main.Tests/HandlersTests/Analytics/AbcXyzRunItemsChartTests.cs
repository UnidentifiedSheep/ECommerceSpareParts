using System.Text.Json;
using Exceptions.Base;
using FluentAssertions;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Charts.AbcXyzRunItems;
using Main.Modules.Analytics.Application.Queries;
using Tests.AnalyticsTests;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Analytics;

public class AbcXyzRunItemsChartTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	private Guid _runId;

	[Fact]
	public async Task Query_ReturnsItemsByRankWithClassesAndData()
	{
		await Seed();

		var result = await Query(new { runId = _runId });

		result.Points.Select(point => point.Rank).Should().Equal(1, 2, 3, 4);
		var first = result.Points[0];
		first.ProductId.Should().Be(101);
		first.Abc.Should().Be(AbcClass.A);
		first.Xyz.Should().Be(XyzClass.X);
		first.Quantity.Should().Be(2m);
		first.Revenue.Should().Be(99m);
		first.PeriodsCount.Should().Be(12);
		result.Points[3].Xyz.Should().BeNull();
		result.NextCursor.Should().Be("4");
	}

	[Fact]
	public async Task Query_PagesByRankCursor()
	{
		await Seed();

		var firstPage = await Query(new { runId = _runId, size = 2 });
		var secondPage = await Query(new { runId = _runId, size = 2, cursor = int.Parse(firstPage.NextCursor!) });
		var emptyPage = await Query(new { runId = _runId, size = 2, cursor = 4 });

		firstPage.Points.Select(point => point.Rank).Should().Equal(1, 2);
		firstPage.NextCursor.Should().Be("2");
		secondPage.Points.Select(point => point.Rank).Should().Equal(3, 4);
		emptyPage.Points.Should().BeEmpty();
		emptyPage.NextCursor.Should().BeNull();
	}

	[Fact]
	public async Task Query_FiltersByAbcAndXyzClasses()
	{
		await Seed();

		var byAbc = await Query(new { runId = _runId, abc = "B" });
		var byXyz = await Query(new { runId = _runId, xyz = "X" });
		var both = await Query(new { runId = _runId, abc = "A", xyz = "Z" });
		var unclassified = await Query(new { runId = _runId, xyz = "Unclassified" });

		byAbc.Points.Select(point => point.ProductId).Should().Equal(103, 104);
		byXyz.Points.Select(point => point.ProductId).Should().Equal(101, 103);
		both.Points.Select(point => point.ProductId).Should().Equal(102);
		unclassified.Points.Select(point => point.ProductId).Should().Equal(104);
	}

	[Fact]
	public async Task Query_ThrowsNotFoundForUnknownRunOrForeignAnalysisType()
	{
		var foreign = await AnalyticsRunTestData.AddRunAsync(Context, CancellationToken, "Other");

		var unknown = async () => await Query(new { runId = Guid.NewGuid() });
		var other = async () => await Query(new { runId = foreign.Id });

		await unknown.Should().ThrowAsync<NotFoundException>();
		await other.Should().ThrowAsync<NotFoundException>();
	}

	[Fact]
	public async Task Query_RejectsEmptyRunIdNegativeCursorAndUnsupportedClasses()
	{
		await Seed();

		var emptyRun = async () => await Query(new { runId = Guid.Empty });
		var cursor = async () => await Query(new { runId = _runId, cursor = -1 });
		var size = async () => await Query(new { runId = _runId, size = 0 });

		await emptyRun.Should().ThrowAsync<Exception>();
		await cursor.Should().ThrowAsync<Exception>();
		await size.Should().ThrowAsync<Exception>();
	}

	[Fact]
	public async Task Charts_ExposeLocalizedSchemasForRunItemsChart()
	{
		var result = await Mediator.Send(new GetChartsQuery(), CancellationToken);

		var chart = result.Charts.Single(item => item.SystemName == AbcXyzRunItemsChartDataSource.DataSourceSystemName);
		chart.Name.Should().NotStartWith("chart.");
		chart.Description.Should().NotStartWith("chart.");
		chart.QueryInputSchema.Fields.Select(field => field.Name)
			.Should()
			.BeEquivalentTo("runId", "abc", "xyz", "cursor", "size");
		chart.QueryInputSchema.Fields.Should().OnlyContain(field => !field.Label!.StartsWith("chart."));
		chart.QueryInputSchema.Fields.Should().OnlyContain(field => !field.Label!.StartsWith("pagination."));
		chart.DataPointSchema.Fields.Should().HaveCount(11);
		chart.DataPointSchema.Fields.Should().OnlyContain(field => !field.Label!.StartsWith("chart."));
		chart.DataPointSchema.Fields.Should().OnlyContain(field => !field.Description!.StartsWith("chart."));
	}

	private async Task Seed()
	{
		var run = await AnalyticsRunTestData.AddRunAsync(Context, CancellationToken);
		_runId = run.Id;
		await AnalyticsRunTestData.AddItemsAsync(
			Context,
			run.Id,
			CancellationToken,
			(101, 1, AbcClass.A, XyzClass.X),
			(102, 2, AbcClass.A, XyzClass.Z),
			(103, 3, AbcClass.B, XyzClass.X),
			(104, 4, AbcClass.B, null));
	}

	private async Task<(IReadOnlyList<AbcXyzRunItemDataPoint> Points, string? NextCursor)> Query(object input)
	{
		var result = await Mediator.Send(
			new QueryChartDataSourceQuery(
				AbcXyzRunItemsChartDataSource.DataSourceSystemName,
				JsonSerializer.Serialize(input)),
			CancellationToken);
		return (result.DataPoints.Cast<AbcXyzRunItemDataPoint>().ToList(), result.NextCursor);
	}
}
