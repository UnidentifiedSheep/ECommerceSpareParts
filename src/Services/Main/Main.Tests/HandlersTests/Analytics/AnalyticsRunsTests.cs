using Application.Common.Models;
using Exceptions.Base;
using FluentAssertions;
using Main.Entities.Analytics;
using Main.Modules.Analytics.Application.Commands;
using Main.Modules.Analytics.Application.Dtos.Runs;
using Main.Modules.Analytics.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Tests.AnalyticsTests;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Analytics;

public class AnalyticsRunsTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	private static readonly Cursor<(Guid Id, DateTime CreatedAt)> FirstPage = new((Guid.Empty, DateTime.MinValue), 100);

	[Fact]
	public async Task GetRuns_ReturnsNewestFirstWithParametersAndSummary()
	{
		var older = await AnalyticsRunTestData.AddRunAsync(Context, CancellationToken, ageInDays: 5);
		var newer = await AnalyticsRunTestData.AddRunAsync(Context, CancellationToken, ageInDays: 1);

		var result = await Mediator.Send(
			new GetAnalyticsRunsQuery(null, null, null, null, FirstPage),
			CancellationToken);

		result.Runs.Select(run => run.Id).Should().Equal(newer.Id, older.Id);
		result.Runs[0].AnalysisType.Should().Be("AbcXyz");
		result.Runs[0].Parameters.Should().Be("{}");
		result.Runs[0].CreatedAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(-1), TimeSpan.FromMinutes(1));
	}

	[Fact]
	public async Task GetRuns_PagesByCursorWithoutRepeatingRuns()
	{
		var first = await AnalyticsRunTestData.AddRunAsync(Context, CancellationToken, ageInDays: 1);
		var second = await AnalyticsRunTestData.AddRunAsync(Context, CancellationToken, ageInDays: 2);
		var third = await AnalyticsRunTestData.AddRunAsync(Context, CancellationToken, ageInDays: 3);

		var page = await Mediator.Send(
			new GetAnalyticsRunsQuery(null, null, null, null, FirstPage with { Size = 2 }),
			CancellationToken);
		var last = page.Runs[^1];
		var next = await Mediator.Send(
			new GetAnalyticsRunsQuery(
				null,
				null,
				null,
				null,
				new Cursor<(Guid Id, DateTime CreatedAt)>((last.Id, last.CreatedAt), 2)),
			CancellationToken);

		page.Runs.Select(run => run.Id).Should().Equal(first.Id, second.Id);
		next.Runs.Select(run => run.Id).Should().Equal(third.Id);
	}

	[Fact]
	public async Task GetRuns_FiltersByTypePinnedAndPeriod()
	{
		var january = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		var pinned = await AnalyticsRunTestData.AddRunAsync(
			Context,
			CancellationToken,
			pinned: true,
			periodStart: january,
			periodEnd: january.AddMonths(1));
		var other = await AnalyticsRunTestData.AddRunAsync(
			Context,
			CancellationToken,
			analysisType: "Other",
			periodStart: january.AddMonths(6),
			periodEnd: january.AddMonths(7));

		var byType = await Send(analysisType: "Other");
		var byPinned = await Send(isPinned: true);
		var unpinned = await Send(isPinned: false);
		var overlapping = await Send(periodFrom: january.AddDays(10), periodTo: january.AddDays(20));
		var outside = await Send(periodFrom: january.AddMonths(2), periodTo: january.AddMonths(3));

		byType.Select(run => run.Id).Should().Equal(other.Id);
		byPinned.Select(run => run.Id).Should().Equal(pinned.Id);
		unpinned.Select(run => run.Id).Should().Equal(other.Id);
		overlapping.Select(run => run.Id).Should().Equal(pinned.Id);
		outside.Should().BeEmpty();
	}

	[Fact]
	public async Task GetRunById_ReturnsRunOrNull()
	{
		var run = await AnalyticsRunTestData.AddRunAsync(Context, CancellationToken);

		var found = await Mediator.Send(new GetAnalyticsRunByIdQuery(run.Id), CancellationToken);
		var missing = await Mediator.Send(new GetAnalyticsRunByIdQuery(Guid.NewGuid()), CancellationToken);

		found.Run.Should().NotBeNull();
		found.Run!.Id.Should().Be(run.Id);
		missing.Run.Should().BeNull();
	}

	[Fact]
	public async Task SetPinned_PinsAndUnpinsRun()
	{
		var run = await AnalyticsRunTestData.AddRunAsync(Context, CancellationToken);

		await Mediator.Send(new SetAnalyticsRunPinnedCommand(run.Id, true), CancellationToken);
		var pinned = await IsPinned(run.Id);
		await Mediator.Send(new SetAnalyticsRunPinnedCommand(run.Id, true), CancellationToken);
		var pinnedAgain = await IsPinned(run.Id);
		await Mediator.Send(new SetAnalyticsRunPinnedCommand(run.Id, false), CancellationToken);
		var unpinned = await IsPinned(run.Id);

		pinned.Should().BeTrue();
		pinnedAgain.Should().BeTrue();
		unpinned.Should().BeFalse();
	}

	[Fact]
	public async Task SetPinned_ThrowsNotFoundForUnknownRun()
	{
		var act = async () => await Mediator.Send(
			new SetAnalyticsRunPinnedCommand(Guid.NewGuid(), true),
			CancellationToken);

		await act.Should().ThrowAsync<NotFoundException>();
	}

	private async Task<bool> IsPinned(Guid runId)
		=> await Context.Set<AnalyticsRun>()
			.AsNoTracking()
			.Where(run => run.Id == runId)
			.Select(run => run.IsPinned)
			.SingleAsync(CancellationToken);

	private async Task<IReadOnlyList<AnalyticsRunDto>> Send(
		string? analysisType = null,
		bool? isPinned = null,
		DateTime? periodFrom = null,
		DateTime? periodTo = null)
		=> (await Mediator.Send(
			new GetAnalyticsRunsQuery(analysisType, isPinned, periodFrom, periodTo, FirstPage),
			CancellationToken)).Runs;
}
