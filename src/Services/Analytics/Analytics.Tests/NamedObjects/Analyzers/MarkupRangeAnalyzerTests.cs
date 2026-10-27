using Analytics.Application.NamedObjects.Analyzers;
using Analytics.Application.NamedObjects.Analyzers.Markup;
using Analytics.Integration.Tests.DataBuilders.Sale;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;

namespace Analytics.Integration.Tests.NamedObjects.Analyzers;

public sealed class MarkupRangeAnalyzerTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	private int _nextId;

	[Fact]
	public async Task EmptyHistory_ReturnsNoRanges()
	{
		var ranges = await Analyze();

		ranges.Should().BeEmpty();
	}

	[Fact]
	public async Task AdjacentBuckets_CoverTheGapBetweenObservedCosts()
	{
		await AddSamples(100, 0.1m, 0.1m);
		await AddSamples(100, 0.2m, 0.1m);
		await AddSamples(100, 0.22m, 0.6m);
		await AddSamples(100, 0.4m, 0.6m);

		var ranges = await Analyze();

		ranges.Should().HaveCount(2);
		ranges[0].FromCost.Should().Be(0.1m);
		ranges[0].ToCost.Should().Be(0.21m);
		ranges[1].FromCost.Should().Be(ranges[0].ToCost);
		ranges[1].ToCost.Should().Be(0.4m);
		ranges.Select(range => range.Count).Should().Equal(200, 200);
	}

	[Fact]
	public async Task SparseHistory_DoesNotSplitOnCostRatioBeforeMinimumSampleSize()
	{
		for (var cost = 1; cost <= 8; cost++)
			await AddSamples(1, cost, 0.2m);

		var ranges = await Analyze();

		ranges.Should().ContainSingle();
		ranges[0].FromCost.Should().Be(1m);
		ranges[0].ToCost.Should().Be(8m);
		ranges[0].Count.Should().Be(8);
	}

	[Fact]
	public async Task SimilarMarkups_AllowWiderCostBuckets()
	{
		await AddSamples(100, 10m, 0.2m);
		await AddSamples(100, 18m, 0.2m);
		await AddSamples(100, 25m, 0.2m);

		var ranges = await Analyze();

		ranges.Should().HaveCount(2);
		ranges.Select(range => range.Count).Should().Equal(200, 100);
		ranges[0].ToCost.Should().Be(21.5m);
		ranges[1].FromCost.Should().Be(21.5m);
	}

	[Fact]
	public async Task VariableMarkups_DoNotProduceBucketsSmallerThanMinimumSampleSize()
	{
		for (var index = 0; index < 200; index++)
			await AddSamples(1, 10m + index * 0.001m, index % 2 == 0 ? 0.1m : 0.9m);

		var ranges = await Analyze();

		ranges.Should().HaveCount(2);
		ranges.Select(range => range.Count).Should().Equal(100, 100);
		ranges[0].ToCost.Should().Be(ranges[1].FromCost);
	}

	[Fact]
	public async Task SmallTail_IsMergedWithPreviousBucketAndStatisticsAreRecalculated()
	{
		await AddSamples(100, 10m, 0.1m);
		await AddSamples(100, 30m, 0.5m);
		await AddSamples(10, 100m, 0.9m);

		var ranges = await Analyze();

		ranges.Should().HaveCount(2);
		ranges.Select(range => range.Count).Should().Equal(100, 110);
		var tail = ranges[1];
		var mean = (100m * 0.5m + 10m * 0.9m) / 110m;
		var variance = (100m * (0.5m - mean) * (0.5m - mean) +
			10m * (0.9m - mean) * (0.9m - mean)) / 109m;
		tail.MeanMarkup.Should().BeApproximately(mean, 0.0000000001m);
		tail.StdDevMarkup.Should().BeApproximately((decimal)Math.Sqrt((double)variance), 0.0000000001m);
		tail.ToCost.Should().Be(100m);
		ranges[0].ToCost.Should().Be(tail.FromCost);
	}

	[Fact]
	public async Task EqualCosts_AreNeverSplitBetweenBuckets()
	{
		await AddSamples(120, 10m, 0.1m);
		await AddSamples(120, 10m, 0.9m);

		var ranges = await Analyze();

		ranges.Should().ContainSingle();
		ranges[0].Count.Should().Be(240);
		ranges[0].FromCost.Should().Be(10m);
		ranges[0].ToCost.Should().Be(10m);
	}

	[Fact]
	public async Task Batches_PreserveBucketAndCoverageAcrossCursorBoundary()
	{
		await AddSamples(1001, 10m, 0.1m);
		await AddSamples(100, 30m, 0.5m);

		var ranges = await Analyze();

		ranges.Should().HaveCount(2);
		ranges.Select(range => range.Count).Should().Equal(1001, 100);
		ranges[0].ToCost.Should().Be(20m);
		ranges[1].FromCost.Should().Be(20m);
	}

	[Fact]
	public async Task Analyze_ExcludesDeletedOutOfPeriodAndInvalidMarkupSamples()
	{
		var date = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
		await AddSamples(1, 10m, 0.2m, date);
		await AddSamples(1, 10m, 0m, date);
		await AddSamples(1, 10m, 4m, date);
		await AddSamples(1, 20m, 0.2m, date.AddDays(-1));
		await AddSamples(1, 30m, 0.2m, date, deleted: true);

		var ranges = await Analyze(new MarkupAnalyzerInput { StartDate = date, EndDate = date });

		ranges.Should().ContainSingle();
		ranges[0].Count.Should().Be(1);
		ranges[0].FromCost.Should().Be(10m);
		ranges[0].ToCost.Should().Be(10m);
	}

	private async Task<IReadOnlyList<MarkupRangeDraft>> Analyze(MarkupAnalyzerInput? input = null)
	{
		await Context.SaveChangesAsync(CancellationToken);
		var analyzer = Scope.ServiceProvider.GetServices<MarkupAnalyzerNamedObjectBase>()
			.OfType<MarkupRangeAnalyzer>().Single();
		return await analyzer.AnalyzeAsync(input ?? new MarkupAnalyzerInput(), CancellationToken);
	}

	private async Task AddSamples(
		int count,
		decimal cost,
		decimal markup,
		DateTime? date = null,
		bool deleted = false)
	{
		var saleId = Guid.NewGuid();
		var contents = Enumerable.Range(0, count).Select(_ =>
		{
			var id = ++_nextId;
			var detail = new SaleContentDetailBuilder(Faker)
				.WithId(id).WithSaleContentId(id).WithCount(1)
				.WithBuyPrice(cost).WithBuyPriceInBaseCurrency(cost).Build();
			return new SaleContentBuilder(Faker)
				.WithId(id).WithSaleId(saleId).WithCount(1)
				.WithPrice(cost * (1 + markup)).WithPriceInBaseCurrency(cost * (1 + markup))
				.WithDetails([detail]).Build();
		}).ToArray();
		var fact = new SalesFactBuilder(Faker).WithId(saleId)
			.WithCreatedAt(date ?? DateTime.UtcNow).WithContents(contents).Build();
		if (deleted)
			fact.MarkDeleted(DateTime.UtcNow);
		await Context.AddAsync(fact, CancellationToken);
	}
}
