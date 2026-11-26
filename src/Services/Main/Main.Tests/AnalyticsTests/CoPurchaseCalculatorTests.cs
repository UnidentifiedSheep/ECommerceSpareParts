using FluentAssertions;
using Main.Modules.Analytics.Abstractions.Models.CoPurchase;
using Main.Modules.Analytics.Application.Analyzers.CoPurchase;
using Main.Modules.Analytics.Application.Models.CoPurchase;

namespace Tests.AnalyticsTests;

public class CoPurchaseCalculatorTests
{
	private const int ProductId = 1;

	private const int TotalSalesCount = 100;

	private readonly CoPurchaseCalculator _calculator = new();

	private static CoPurchaseParameters Parameters()
		=> new()
		{
			From = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc),
			To = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc)
		};

	private static IEnumerable<CoPurchaseSaleProduct> Sale(params int[] productIds)
	{
		var saleId = Guid.NewGuid();
		return productIds.Select(productId => new CoPurchaseSaleProduct(saleId, productId));
	}

	private static IEnumerable<CoPurchaseSaleProduct> Sales(int count, params int[] productIds)
		=> Enumerable.Range(0, count).SelectMany(_ => Sale(productIds));

	private static Guid[] SaleIds(int count) => Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToArray();

	private static CoPurchaseCandidate Candidate(int productId, params Guid[] saleIds)
		=> new(productId, saleIds.ToHashSet());

	private static CoPurchaseCandidates Candidates(int salesCount, params CoPurchaseCandidate[] candidates)
		=> new(ProductId, new HashSet<int> { ProductId }, salesCount, candidates);

	private CoPurchaseCandidates Select(
		IEnumerable<CoPurchaseSaleProduct> sales,
		IReadOnlySet<int>? crossIds = null,
		CoPurchaseParameters? parameters = null)
		=> _calculator.SelectCandidates(
			ProductId,
			crossIds ?? new HashSet<int>(),
			sales.ToList(),
			TotalSalesCount,
			parameters ?? Parameters());

	private IReadOnlyList<CoPurchaseRelated> Build(
		CoPurchaseCandidates candidates,
		IReadOnlyDictionary<int, int> productSalesCounts,
		IReadOnlyDictionary<int, IReadOnlySet<int>>? crosses = null,
		CoPurchaseParameters? parameters = null)
		=> _calculator.BuildRelated(
			candidates,
			crosses ?? new Dictionary<int, IReadOnlySet<int>>(),
			productSalesCounts,
			TotalSalesCount,
			parameters ?? Parameters());

	[Fact]
	public void SelectCandidates_PoolsCrossSalesAndExcludesAnalogs()
	{
		var sales = Sales(3, ProductId, 3)
			.Concat(Sales(2, 2, 3))
			.Concat(Sale(ProductId, 2));

		var result = Select(sales, new HashSet<int> { 2 });

		result.Members.Should().BeEquivalentTo([ProductId, 2]);
		result.SalesCount.Should().Be(6);
		result.Candidates.Should().ContainSingle();
		result.Candidates[0].ProductId.Should().Be(3);
		result.Candidates[0].SaleIds.Should().HaveCount(5);
	}

	[Fact]
	public void SelectCandidates_CountsSaleWithProductAndCrossOnce()
	{
		var sales = Sale(ProductId, 2, 3).Concat(Sale(ProductId, 3));

		var result = Select(sales, new HashSet<int> { 2 });

		result.SalesCount.Should().Be(2);
		result.Candidates.Should().ContainSingle().Which.SaleIds.Should().HaveCount(2);
	}

	[Fact]
	public void SelectCandidates_TreatsCrossBoughtTogetherAsCandidate()
	{
		var sales = Sales(3, ProductId, 2).Concat(Sale(ProductId)).Concat(Sale(2, 4));

		var result = Select(sales, new HashSet<int> { 2 });

		result.Members.Should().BeEquivalentTo([ProductId]);
		result.SalesCount.Should().Be(4);
		result.Candidates.Should().ContainSingle();
		result.Candidates[0].ProductId.Should().Be(2);
		result.Candidates[0].SaleIds.Should().HaveCount(3);
	}

	[Fact]
	public void SelectCandidates_KeepsCrossBelowSuspiciousLift()
	{
		var sales = Sales(3, ProductId, 2).Concat(Sale(ProductId)).Concat(Sale(2, 4));

		var result = Select(sales, new HashSet<int> { 2 }, Parameters() with { SuspiciousCrossMinLift = 100m });

		result.Members.Should().BeEquivalentTo([ProductId, 2]);
		result.Candidates.Should().ContainSingle().Which.ProductId.Should().Be(4);
	}

	[Fact]
	public void SelectCandidates_KeepsCrossBelowSuspiciousPairCount()
	{
		var sales = Sales(2, ProductId, 2).Concat(Sale(ProductId, 3));

		var result = Select(sales, new HashSet<int> { 2 });

		result.Members.Should().BeEquivalentTo([ProductId, 2]);
		result.Candidates.Select(candidate => candidate.ProductId).Should().Equal(3);
	}

	[Fact]
	public void SelectCandidates_TakesTopKByCountThenProductId()
	{
		var sales = Sales(3, ProductId, 5)
			.Concat(Sales(2, ProductId, 7))
			.Concat(Sales(2, ProductId, 6))
			.Concat(Sale(ProductId, 4));

		var result = Select(sales, parameters: Parameters() with { CandidatesTopK = 3, ResultTopN = 1 });

		result.Candidates.Select(candidate => candidate.ProductId).Should().Equal(5, 6, 7);
	}

	[Fact]
	public void BuildRelated_CollapsesCandidateCrossesAndCountsDistinctSales()
	{
		var sales = SaleIds(4);
		var candidates = Candidates(
			10,
			Candidate(10, sales[0], sales[1], sales[2]),
			Candidate(12, SaleIds(3)),
			Candidate(11, sales[2], sales[3]));
		var crosses = new Dictionary<int, IReadOnlySet<int>> { [11] = new HashSet<int> { 10 } };

		var result = Build(candidates, new Dictionary<int, int> { [10] = 20, [11] = 10, [12] = 15 }, crosses);

		result.Should().HaveCount(2);
		result[0].Members.Should().Equal(new CoPurchaseRelatedMember(10, 3), new CoPurchaseRelatedMember(11, 2));
		result[0].PairCount.Should().Be(4);
		result[0].Confidence.Should().Be(0.4m);
		result[0].Lift.Should().Be(1.333333m);
		result[1].Members.Should().Equal(new CoPurchaseRelatedMember(12, 3));
		result[1].Confidence.Should().Be(0.3m);
		result[1].Lift.Should().Be(2m);
	}

	[Fact]
	public void BuildRelated_AppliesMinPairCountToCollapsedGroup()
	{
		var candidates = Candidates(
			10,
			Candidate(10, SaleIds(2)),
			Candidate(11, SaleIds(1)),
			Candidate(12, SaleIds(2)));
		var crosses = new Dictionary<int, IReadOnlySet<int>> { [10] = new HashSet<int> { 11 } };

		var result = Build(candidates, new Dictionary<int, int> { [10] = 2, [11] = 1, [12] = 2 }, crosses);

		result.Should().ContainSingle().Which.Members.Select(member => member.ProductId).Should().Equal(10, 11);
	}

	[Fact]
	public void BuildRelated_SkipsRelationsWithoutPositiveLift()
	{
		var candidates = Candidates(10, Candidate(10, SaleIds(3)));

		var result = Build(candidates, new Dictionary<int, int> { [10] = 30 });

		result.Should().BeEmpty();
	}

	[Fact]
	public void BuildRelated_UsesPairCountWhenSalesCountIsMissing()
	{
		var candidates = Candidates(10, Candidate(10, SaleIds(3)));

		var result = Build(candidates, new Dictionary<int, int>());

		result.Should().ContainSingle().Which.Lift.Should().Be(10m);
	}

	[Fact]
	public void BuildRelated_TakesTopNByConfidence()
	{
		var candidates = Candidates(
			20,
			Candidate(10, SaleIds(3)),
			Candidate(11, SaleIds(5)),
			Candidate(12, SaleIds(4)));

		var result = Build(
			candidates,
			new Dictionary<int, int> { [10] = 3, [11] = 5, [12] = 4 },
			parameters: Parameters() with { ResultTopN = 2 });

		result.Select(item => item.Members[0].ProductId).Should().Equal(11, 12);
	}

	[Fact]
	public void BuildRelated_ReturnsEmptyWhenProductHasNoSales()
		=> Build(Candidates(0, Candidate(10, SaleIds(3))), new Dictionary<int, int> { [10] = 3 })
			.Should()
			.BeEmpty();
}
