using System.Text.Json;
using FluentAssertions;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Analyzers.AbcXyz;
using Main.Modules.Analytics.Application.Models;
using Main.Modules.Analytics.Application.Services;

namespace Tests.AnalyticsTests;

public class AbcXyzRunFactoryTests
{
	private static readonly DateTime From = new(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc);

	private static AbcXyzParameters Parameters()
		=> new()
		{
			From = From,
			To = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
			AbcMetric = AnalysisMetric.Revenue,
			XyzMetric = AnalysisMetric.Quantity,
			AShare = 0.80m,
			BShare = 0.15m,
			XCoefficient = 0.10m,
			YCoefficient = 0.25m,
			MinPeriods = 6
		};

	private static AbcXyzAnalysisResult Result()
	{
		var items = new[]
		{
			new AbcXyzItem(7, 1, 12m, 900m, 900m, 0.9m, 0.9m, AbcClass.A, XyzClass.X, 0.05m, 12),
			new AbcXyzItem(3, 2, 2m, 100m, 100m, 0.1m, 1m, AbcClass.B, null, null, 3)
		};
		return new AbcXyzAnalysisResult(
			items,
			new AbcXyzSummary(2, 12, 1000m, [new AbcXyzCell(AbcClass.A, XyzClass.X, 1, 900m)]));
	}

	[Fact]
	public void Create_FillsRunFromParametersAndResult()
	{
		var jobId = Guid.NewGuid();
		var userId = Guid.NewGuid();

		var draft = AbcXyzRunFactory.Create(Parameters(), Result(), jobId, userId);

		draft.Run.AnalysisType.Should().Be("AbcXyz");
		draft.Run.SchemaVersion.Should().Be(2);
		draft.Run.JobId.Should().Be(jobId);
		draft.Run.CreatedBy.Should().Be(userId);
		draft.Run.PeriodStart.Should().Be(From);
		draft.Run.PeriodEnd.Should().Be(new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc));
		draft.Run.ItemsCount.Should().Be(2);
		draft.Run.CompletedAt.Should().NotBeNull();
		draft.Run.ParametersHash.Should().HaveLength(64);
		JsonDocument.Parse(draft.Run.Parameters).RootElement.GetProperty("abcMetric").GetString()
			.Should()
			.Be("Revenue");
		JsonDocument.Parse(draft.Run.Summary!).RootElement.GetProperty("cells")[0].GetProperty("abc")
			.GetString()
			.Should()
			.Be("A");
		JsonDocument.Parse(draft.Run.Summary!).RootElement.GetProperty("cells")[0].GetProperty("xyz")
			.GetString()
			.Should()
			.Be("X");
	}

	[Fact]
	public void Create_StoresMonthsAsNumbersAndReadsThemBack()
	{
		var draft = AbcXyzRunFactory.Create(
			Parameters() with { Months = [Month.December, Month.January, Month.January] },
			Result(),
			null,
			null);

		JsonDocument.Parse(draft.Run.Parameters).RootElement.GetProperty("months").EnumerateArray()
			.Select(month => month.GetInt32())
			.Should()
			.Equal(1, 12);
		JsonSerializer.Deserialize<AbcXyzParameters>(draft.Run.Parameters)!.Months
			.Should()
			.Equal(Month.January, Month.December);
	}

	[Fact]
	public void Create_StoresItemsWithSegmentsAndData()
	{
		var draft = AbcXyzRunFactory.Create(Parameters(), Result(), null, null);

		draft.Items.Should().HaveCount(2);
		var classified = draft.Items[0];
		classified.RunId.Should().Be(draft.Run.Id);
		classified.SubjectKey.Should().Be("7");
		classified.Rank.Should().Be(1);
		classified.Value.Should().Be(900m);
		classified.Share.Should().Be(0.9m);
		classified.Segments.Should().Be("""{"abc":"A","xyz":"X"}""");
		var data = JsonDocument.Parse(classified.Data).RootElement;
		data.GetProperty("quantity").GetDecimal().Should().Be(12m);
		data.GetProperty("revenue").GetDecimal().Should().Be(900m);
		data.GetProperty("cumulativeShare").GetDecimal().Should().Be(0.9m);
		data.GetProperty("coefficientOfVariation").GetDecimal().Should().Be(0.05m);
		data.GetProperty("periodsCount").GetInt32().Should().Be(12);
	}

	[Fact]
	public void Create_OmitsXyzSegmentAndCoefficientWhenUnclassified()
	{
		var draft = AbcXyzRunFactory.Create(Parameters(), Result(), null, null);

		var unclassified = draft.Items[1];
		unclassified.Segments.Should().Be("""{"abc":"B"}""");
		JsonDocument.Parse(unclassified.Data).RootElement.TryGetProperty("coefficientOfVariation", out _)
			.Should()
			.BeFalse();
	}

	[Fact]
	public void ItemModel_ReadsBackWhatTheFactoryStored()
	{
		var draft = AbcXyzRunFactory.Create(Parameters(), Result(), null, null);

		var classified = AnalyticsRunItemModel<AbcXyzItemSegments, AbcXyzItemData>.From(draft.Items[0]);
		var unclassified = AnalyticsRunItemModel<AbcXyzItemSegments, AbcXyzItemData>.From(draft.Items[1]);

		classified.Should()
			.Be(
				new AnalyticsRunItemModel<AbcXyzItemSegments, AbcXyzItemData>(
					"7",
					1,
					900m,
					0.9m,
					new AbcXyzItemSegments(AbcClass.A, XyzClass.X),
					new AbcXyzItemData(12m, 900m, 0.9m, 0.05m, 12)));
		unclassified.Segments.Should().Be(new AbcXyzItemSegments(AbcClass.B, null));
		unclassified.Data.CoefficientOfVariation.Should().BeNull();
	}

	[Fact]
	public void ItemModel_ToEntityKeepsRunLinkAndColumns()
	{
		var runId = Guid.NewGuid();
		var model = new AnalyticsRunItemModel<AbcXyzItemSegments, AbcXyzItemData>(
			"5",
			3,
			10m,
			0.1m,
			new AbcXyzItemSegments(AbcClass.C, XyzClass.Z),
			new AbcXyzItemData(1m, 10m, 1m, 0.4m, 6));

		var entity = model.ToEntity(runId);

		entity.RunId.Should().Be(runId);
		entity.SubjectKey.Should().Be("5");
		entity.Rank.Should().Be(3);
		entity.Segments.Should().Be("""{"abc":"C","xyz":"Z"}""");
	}

	[Fact]
	public void Hash_IgnoresMonthOrderAndDuplicatesAndEmptyMonths()
	{
		var first = Parameters() with { Months = [Month.December, Month.January, Month.January] };
		var second = Parameters() with { Months = [Month.January, Month.December] };

		AbcXyzRunFactory.ComputeHash(first).Should().Be(AbcXyzRunFactory.ComputeHash(second));
		AbcXyzRunFactory.ComputeHash(Parameters() with { Months = [] })
			.Should()
			.Be(AbcXyzRunFactory.ComputeHash(Parameters()));
	}

	[Fact]
	public void Hash_DiffersWhenParametersDiffer()
		=> AbcXyzRunFactory.ComputeHash(Parameters())
			.Should()
			.NotBe(AbcXyzRunFactory.ComputeHash(Parameters() with { MinPeriods = 5 }));

	[Fact]
	public void Hash_IsTheSameForDerivedInputState()
	{
		var state = new Main.Modules.Analytics.Application.Lrts.AbcXyzCalculation.AbcXyzCalculationInputState
		{
			From = From,
			To = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
			AbcMetric = AnalysisMetric.Revenue,
			XyzMetric = AnalysisMetric.Quantity,
			AShare = 0.80m,
			BShare = 0.15m,
			XCoefficient = 0.10m,
			YCoefficient = 0.25m,
			MinPeriods = 6
		};

		AbcXyzRunFactory.ComputeHash(state).Should().Be(AbcXyzRunFactory.ComputeHash(Parameters()));
	}
}
