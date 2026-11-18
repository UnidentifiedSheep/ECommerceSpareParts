using FluentAssertions;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Analyzers.AbcXyz;
using Main.Modules.Analytics.Application.Models;

namespace Tests.AnalyticsTests;

public class AbcXyzCalculatorTests
{
	private static readonly DateTime WindowStart = new(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc);

	private readonly AbcXyzCalculator _calculator = new();

	private static AbcXyzParameters Parameters()
		=> new()
		{
			From = WindowStart,
			To = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
			AbcMetric = AnalysisMetric.Revenue,
			XyzMetric = AnalysisMetric.Quantity,
			AShare = 0.80m,
			BShare = 0.15m,
			XCoefficient = 0.10m,
			YCoefficient = 0.25m,
			MinPeriods = 6
		};

	private static ProductPeriodValue Row(int productId, int monthIndex, decimal quantity, decimal revenue)
		=> new(productId, WindowStart.AddMonths(monthIndex), quantity, revenue);

	private static IEnumerable<ProductPeriodValue> Constant(int productId, int from, int to, decimal quantity)
		=> Enumerable.Range(from, to - from).Select(month => Row(productId, month, quantity, quantity));

	[Fact]
	public void Abc_ClassifiesByCumulativeShareOfRevenue()
	{
		var result = _calculator.Calculate(
			[Row(1, 0, 1, 800), Row(2, 0, 1, 150), Row(3, 0, 1, 50)],
			Parameters());

		result.Items.Select(item => (item.ProductId, item.Rank, item.AbcClass))
			.Should()
			.Equal(
				(1, 1, AbcClass.A),
				(2, 2, AbcClass.B),
				(3, 3, AbcClass.C));
		result.Items.Select(item => item.Share).Should().Equal(0.8m, 0.15m, 0.05m);
		result.Items[^1].CumulativeShare.Should().Be(1m);
	}

	[Fact]
	public void Abc_DominantProductStaysInClassA()
	{
		var result = _calculator.Calculate(
			[Row(1, 0, 1, 900), Row(2, 0, 1, 50), Row(3, 0, 1, 50)],
			Parameters());

		result.Items.Select(item => item.AbcClass).Should().Equal(AbcClass.A, AbcClass.B, AbcClass.C);
	}

	[Fact]
	public void Abc_EqualValuesAreOrderedByProductId()
	{
		var result = _calculator.Calculate(
			[Row(7, 0, 1, 100), Row(3, 0, 1, 100)],
			Parameters());

		result.Items.Select(item => item.ProductId).Should().Equal(3, 7);
	}

	[Fact]
	public void Abc_ProductWithoutRevenueGoesToClassCWithZeroShare()
	{
		var result = _calculator.Calculate(
			[Row(1, 0, 1, 100), Row(2, 0, 5, 0)],
			Parameters());

		var empty = result.Items.Single(item => item.ProductId == 2);
		empty.AbcClass.Should().Be(AbcClass.C);
		empty.Share.Should().Be(0m);
		result.Summary.TotalValue.Should().Be(100m);
	}

	[Fact]
	public void Abc_QuantityCriterionRanksByQuantity()
	{
		var result = _calculator.Calculate(
			[Row(1, 0, 5, 1000), Row(2, 0, 50, 10)],
			Parameters() with { AbcMetric = AnalysisMetric.Quantity });

		result.Items.Select(item => item.ProductId).Should().Equal(2, 1);
		result.Items[0].Value.Should().Be(50m);
	}

	[Fact]
	public void Xyz_StableDemandIsClassX()
	{
		var result = _calculator.Calculate(Constant(1, 0, 12, 10), Parameters());

		var item = result.Items.Single();
		item.XyzClass.Should().Be(XyzClass.X);
		item.CoefficientOfVariation.Should().Be(0m);
		item.PeriodsCount.Should().Be(12);
	}

	[Theory]
	[InlineData(9, 11, XyzClass.X)]
	[InlineData(8, 12, XyzClass.Y)]
	[InlineData(0, 20, XyzClass.Z)]
	public void Xyz_ClassifiesByCoefficientOfVariation(decimal low, decimal high, XyzClass expected)
	{
		var rows = Enumerable.Range(0, 12).Select(month => Row(1, month, month % 2 == 0 ? high : low, 1));

		var result = _calculator.Calculate(rows, Parameters());

		result.Items.Single().XyzClass.Should().Be(expected);
	}

	[Fact]
	public void Xyz_PeriodsWithoutSalesAfterFirstSaleCountAsZero()
	{
		var result = _calculator.Calculate([Row(1, 0, 12, 12)], Parameters());

		var item = result.Items.Single();
		item.PeriodsCount.Should().Be(12);
		item.CoefficientOfVariation.Should().BeApproximately(3.3166247m, 0.000001m);
		item.XyzClass.Should().Be(XyzClass.Z);
	}

	[Fact]
	public void Xyz_PeriodsBeforeFirstSaleAreIgnored()
	{
		var result = _calculator.Calculate(Constant(1, 4, 12, 10), Parameters());

		var item = result.Items.Single();
		item.PeriodsCount.Should().Be(8);
		item.XyzClass.Should().Be(XyzClass.X);
	}

	[Fact]
	public void Xyz_TooShortHistoryIsNotClassified()
	{
		var result = _calculator.Calculate(Constant(1, 8, 12, 10), Parameters());

		var item = result.Items.Single();
		item.PeriodsCount.Should().Be(4);
		item.XyzClass.Should().BeNull();
		item.CoefficientOfVariation.Should().BeNull();
	}

	[Fact]
	public void Seasons_OnlySelectedMonthsAreAnalyzed()
	{
		var parameters = Parameters() with
		{
			From = new DateTime(2024, 11, 1, 0, 0, 0, DateTimeKind.Utc),
			To = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
			Months = [Month.December, Month.January, Month.February]
		};
		var season = new[]
		{
			new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc),
			new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
			new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc),
			new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
		};
		var rows = season
			.Select(month => new ProductPeriodValue(1, month, 10, 10))
			.Append(new ProductPeriodValue(1, new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc), 999, 999))
			.ToList();

		var result = _calculator.Calculate(rows, parameters);

		var item = result.Items.Single();
		item.Quantity.Should().Be(60m);
		item.PeriodsCount.Should().Be(6);
		item.XyzClass.Should().Be(XyzClass.X);
		result.Summary.PeriodsCount.Should().Be(6);
	}

	[Fact]
	public void Summary_ContainsFullMatrixWithAllProducts()
	{
		var rows = Constant(1, 0, 12, 10)
			.Concat([Row(2, 11, 1, 5)])
			.ToList();

		var result = _calculator.Calculate(rows, Parameters());

		result.Summary.Cells.Should().HaveCount(12);
		result.Summary.Cells.Sum(cell => cell.ProductsCount).Should().Be(result.Items.Count);
		result.Summary.ProductsCount.Should().Be(2);
		result.Summary.Cells.Sum(cell => cell.Value).Should().Be(result.Summary.TotalValue);
	}

	[Fact]
	public void DemandPattern_ClassifiesByAdiAndCv2()
	{
		var rows = Constant(1, 0, 12, 10m)
			.Concat([Row(2, 0, 1, 1), Row(2, 1, 30, 30)])
			.Concat(Enumerable.Range(2, 10).Select(month => Row(2, month, month % 2 == 0 ? 1 : 30, 1)))
			.Concat([Row(3, 0, 10, 10), Row(3, 6, 10, 10), Row(3, 11, 10, 10)])
			.Concat([Row(4, 0, 1, 1), Row(4, 5, 50, 50), Row(4, 11, 5, 5)]);

		var result = _calculator.Calculate(rows, Parameters());

		var items = result.Items.ToDictionary(item => item.ProductId);
		items[1].DemandPattern.Should().Be(DemandPattern.Smooth);
		items[1].Adi.Should().Be(1m);
		items[1].Cv2.Should().Be(0m);
		items[2].DemandPattern.Should().Be(DemandPattern.Erratic);
		items[3].DemandPattern.Should().Be(DemandPattern.Intermittent);
		items[3].Adi.Should().Be(4m);
		items[4].DemandPattern.Should().Be(DemandPattern.Lumpy);
	}

	[Fact]
	public void DemandPattern_IsEmptyForSingleSaleAndTooShortHistory()
	{
		var result = _calculator.Calculate([Row(1, 0, 10, 10), Row(2, 8, 5, 5), Row(2, 9, 5, 5)], Parameters());

		var single = result.Items.Single(item => item.ProductId == 1);
		single.DemandPattern.Should().BeNull();
		single.Adi.Should().Be(12m);
		single.Cv2.Should().BeNull();
		var shortHistory = result.Items.Single(item => item.ProductId == 2);
		shortHistory.DemandPattern.Should().BeNull();
		shortHistory.Adi.Should().BeNull();
	}

	[Fact]
	public void DemandPattern_UsesConfiguredCutoffs()
	{
		IEnumerable<ProductPeriodValue> rows =
			[Row(1, 0, 10, 10), Row(1, 1, 20, 20), Row(1, 2, 10, 10), Row(1, 3, 20, 20)];

		DemandPattern? Classify(AbcXyzParameters parameters)
			=> _calculator.Calculate(rows, parameters).Items.Single().DemandPattern;

		Classify(Parameters()).Should().Be(DemandPattern.Intermittent);
		Classify(Parameters() with { Cv2Cutoff = 0.01m }).Should().Be(DemandPattern.Lumpy);
		Classify(Parameters() with { AdiCutoff = 5m }).Should().Be(DemandPattern.Smooth);
		Classify(Parameters() with { AdiCutoff = 5m, Cv2Cutoff = 0.01m }).Should().Be(DemandPattern.Erratic);
	}

	[Fact]
	public void Quarterly_AggregatesMonthsIntoQuarterPeriods()
	{
		var parameters = Parameters() with
		{
			From = new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc),
			To = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
			Granularity = AnalysisPeriodGranularity.Quarter,
			MinPeriods = 2
		};

		var result = _calculator.Calculate([Row(1, 0, 10, 10), Row(1, 1, 20, 20), Row(1, 5, 30, 30)], parameters);

		result.Summary.PeriodsCount.Should().Be(4);
		var item = result.Items.Single();
		item.Quantity.Should().Be(60m);
		item.PeriodsCount.Should().Be(4);
		item.Adi.Should().Be(2m);
	}

	[Fact]
	public void Summary_ContainsPatternMatrixCoveringEveryProduct()
	{
		var result = _calculator.Calculate(
			[Row(1, 0, 10, 100), Row(2, 3, 5, 50), Row(3, 4, 2, 10)],
			Parameters());

		var cells = result.Summary.PatternCells!;
		cells.Should().HaveCount(Enum.GetValues<AbcClass>().Length * (Enum.GetValues<DemandPattern>().Length + 1));
		cells.Sum(cell => cell.ProductsCount).Should().Be(result.Items.Count);
	}

	[Fact]
	public void Validator_AcceptsQuarterlyWindowOfWholeQuarters()
		=> Validate(QuarterlyParameters()).IsValid.Should().BeTrue();

	[Theory]
	[MemberData(nameof(InvalidQuarterlyParameters))]
	public void Validator_RejectsInvalidQuarterlyAndCutoffParameters(AbcXyzParameters parameters)
		=> Validate(parameters).IsValid.Should().BeFalse();

	public static TheoryData<AbcXyzParameters> InvalidQuarterlyParameters()
		=> new()
		{
			QuarterlyParameters() with { From = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc) },
			QuarterlyParameters() with { To = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc) },
			QuarterlyParameters() with { Months = [Month.January] },
			QuarterlyParameters() with { MinPeriods = 5 },
			Parameters() with { AdiCutoff = 0m },
			Parameters() with { Cv2Cutoff = -1m },
			Parameters() with { Granularity = (AnalysisPeriodGranularity)99 }
		};

	private static AbcXyzParameters QuarterlyParameters()
		=> Parameters() with
		{
			From = new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc),
			To = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
			Granularity = AnalysisPeriodGranularity.Quarter,
			MinPeriods = 2
		};

	[Fact]
	public void Validator_AcceptsValidParameters()
		=> Validate(Parameters()).IsValid.Should().BeTrue();

	[Fact]
	public void Validator_AcceptsWindowWithExactlyMinPeriods()
		=> Validate(Parameters() with { To = WindowStart.AddMonths(5) }).IsValid.Should().BeTrue();

	[Fact]
	public void Validator_AcceptsSelectedMonthsAndOptionalFilters()
		=> Validate(
				Parameters() with
				{
					Months = [Month.January, Month.December],
					MinPeriods = 2,
					OrganizationId = Guid.NewGuid(),
					BuyerId = Guid.NewGuid()
				})
			.IsValid.Should()
			.BeTrue();

	[Theory]
	[MemberData(nameof(InvalidParameters))]
	public void Validator_RejectsInvalidParameters(AbcXyzParameters parameters)
		=> Validate(parameters).IsValid.Should().BeFalse();

	public static TheoryData<AbcXyzParameters> InvalidParameters()
		=> new()
		{
			Parameters() with { From = new DateTime(2025, 11, 15, 0, 0, 0, DateTimeKind.Utc) },
			Parameters() with { From = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Unspecified) },
			Parameters() with { To = new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc) },
			Parameters() with { To = WindowStart.AddMonths(-1) },
			Parameters() with { Months = [(Month)0] },
			Parameters() with { Months = [(Month)13] },
			Parameters() with { AbcMetric = (AnalysisMetric)99 },
			Parameters() with { XyzMetric = (AnalysisMetric)99 },
			Parameters() with { AShare = 0m },
			Parameters() with { BShare = 0m },
			Parameters() with { AShare = 0.9m, BShare = 0.2m },
			Parameters() with { XCoefficient = 0m },
			Parameters() with { XCoefficient = 0.3m, YCoefficient = 0.2m },
			Parameters() with { MinPeriods = 1 },
			Parameters() with { To = WindowStart.AddMonths(4) },
			Parameters() with { To = WindowStart },
			Parameters() with { Months = [Month.January, Month.February], MinPeriods = 12 }
		};

	private static FluentValidation.Results.ValidationResult Validate(AbcXyzParameters parameters)
		=> new AbcXyzParametersValidator().Validate(parameters);
}
