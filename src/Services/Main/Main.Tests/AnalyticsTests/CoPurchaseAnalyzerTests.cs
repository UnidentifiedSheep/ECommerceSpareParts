using FluentAssertions;
using Main.Modules.Analytics.Abstractions.Models.CoPurchase;
using Main.Modules.Analytics.Application.Analyzers.CoPurchase;
using Main.Modules.Analytics.Application.Models.CoPurchase;
using Main.Modules.Analytics.Application.Services;
using Main.Modules.Product.Abstractions;
using Moq;

namespace Tests.AnalyticsTests;

public class CoPurchaseAnalyzerTests
{
	private const int TotalSalesCount = 100;

	private readonly Mock<ICoPurchaseDataReader> _reader = new();

	private readonly Mock<IProductCrosses> _crosses = new();

	private readonly List<CoPurchaseSaleProduct> _sales = [];

	public CoPurchaseAnalyzerTests()
	{
		_reader
			.Setup(reader => reader.GetSalesAsync(
				It.IsAny<CoPurchaseParameters>(),
				It.IsAny<IReadOnlyCollection<int>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((CoPurchaseParameters _, IReadOnlyCollection<int> ids, CancellationToken _) =>
			{
				var saleIds = _sales.Where(row => ids.Contains(row.ProductId)).Select(row => row.SaleId).ToHashSet();
				return _sales.Where(row => saleIds.Contains(row.SaleId)).ToList();
			});
		_reader
			.Setup(reader => reader.GetSalesCountsAsync(
				It.IsAny<CoPurchaseParameters>(),
				It.IsAny<IReadOnlyCollection<int>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((CoPurchaseParameters _, IReadOnlyCollection<int> ids, CancellationToken _) =>
				ids.ToDictionary(id => id, id => _sales.Where(row => row.ProductId == id).Select(row => row.SaleId).Distinct().Count()));
		_crosses
			.Setup(crosses => crosses.GetCrossIdsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(Array.Empty<int>());
	}

	private static CoPurchaseParameters Parameters()
		=> new()
		{
			From = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc),
			To = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc)
		};

	private void AddSales(int count, params int[] productIds)
	{
		for (var i = 0; i < count; i++)
		{
			var saleId = Guid.NewGuid();
			_sales.AddRange(productIds.Select(productId => new CoPurchaseSaleProduct(saleId, productId)));
		}
	}

	private void SetCrosses(int productId, params int[] crossIds)
		=> _crosses
			.Setup(crosses => crosses.GetCrossIdsAsync(productId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(crossIds);

	private Task<IReadOnlyList<CoPurchaseProductResult>> Analyze(params int[] productIds)
		=> new CoPurchaseAnalyzer(_reader.Object, new CoPurchaseCalculator(), _crosses.Object)
			.AnalyzeAsync(productIds, TotalSalesCount, Parameters());

	[Fact]
	public async Task Analyze_ReusesSalesAndResultForProductsWithSameCrossScope()
	{
		SetCrosses(1, 2);
		SetCrosses(2, 1);
		AddSales(3, 1, 10);
		AddSales(2, 2, 10);

		var results = await Analyze(1, 2);

		results.Select(result => result.ProductId).Should().Equal(1, 2);
		results[0].SalesCount.Should().Be(5);
		results[0].Related.Should().ContainSingle().Which.PairCount.Should().Be(5);
		results[1].Related.Should().BeSameAs(results[0].Related);
		_reader.Verify(
			reader => reader.GetSalesAsync(
				It.IsAny<CoPurchaseParameters>(),
				It.IsAny<IReadOnlyCollection<int>>(),
				It.IsAny<CancellationToken>()),
			Times.Once);
		_reader.Verify(
			reader => reader.GetSalesCountsAsync(
				It.IsAny<CoPurchaseParameters>(),
				It.IsAny<IReadOnlyCollection<int>>(),
				It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Analyze_CollapsesCandidatesByTheirCrosses()
	{
		SetCrosses(10, 11);
		AddSales(3, 1, 10);
		AddSales(2, 1, 11);

		var results = await Analyze(1);

		var related = results.Should().ContainSingle().Which.Related.Should().ContainSingle().Subject;
		related.Members.Select(member => member.ProductId).Should().Equal(10, 11);
		related.PairCount.Should().Be(5);
	}

	[Fact]
	public async Task Analyze_DoesNotReadSalesCountsWithoutCandidates()
	{
		AddSales(3, 1);

		var results = await Analyze(1);

		results.Should().ContainSingle().Which.Related.Should().BeEmpty();
		results[0].SalesCount.Should().Be(3);
		_reader.Verify(
			reader => reader.GetSalesCountsAsync(
				It.IsAny<CoPurchaseParameters>(),
				It.IsAny<IReadOnlyCollection<int>>(),
				It.IsAny<CancellationToken>()),
			Times.Never);
	}
}
