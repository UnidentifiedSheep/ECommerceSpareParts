using FluentAssertions;
using Main.Entities.Sale;
using Main.Entities.Sale.Sale;
using DomainSale = Main.Entities.Sale.Sale.Sale;

namespace Tests.Domain.Sale;

public class SaleContentAllocatorTests
{
	private static readonly DateTime PurchaseDate = new(2026, 1, 1);

	[Fact]
	public void Distribute_TakesLargestLotsFirstAndSplitsTheLastOne()
	{
		var result = SaleContentAllocator.Distribute(
			[Lot(1, 10, 2, 100m), Lot(2, 10, 5, 80m), Lot(3, 10, 5, 120m)],
			[Line(0, 10, 7)]);

		var content = result.Should().ContainSingle().Which;
		content.Count.Should().Be(7);
		content.TotalSum.Should().Be(1260m);
		content.Details.Select(detail => (detail.StorageContentId, detail.Count))
			.Should()
			.Equal((3, 5), (2, 2));
	}

	[Fact]
	public void Distribute_DoesNotReuseQuantityBetweenLines()
	{
		var result = SaleContentAllocator.Distribute(
			[Lot(1, 10, 6, 100m)],
			[Line(0, 10, 4), Line(1, 10, 2)]);

		result.Sum(content => content.Details.Sum(detail => detail.Count)).Should().Be(6);
		result.Select(content => content.Count).Should().Equal(4, 2);
	}

	[Fact]
	public void Distribute_KeepsTheOriginalLineOrder()
	{
		var result = SaleContentAllocator.Distribute(
			[Lot(1, 10, 1, 100m), Lot(2, 20, 5, 100m)],
			[Line(0, 10, 1), Line(1, 20, 5)]);

		result.Select(content => content.ProductId).Should().Equal(10, 20);
	}

	[Fact]
	public void Distribute_WhenStorageIsNotEnough_Throws()
	{
		var act = () => SaleContentAllocator.Distribute(
			[Lot(1, 10, 2, 100m)],
			[Line(0, 10, 3)]);

		act.Should().Throw<InvalidOperationException>();
	}

	[Fact]
	public void Distribute_WhenProductHasNoLots_Throws()
	{
		var act = () => SaleContentAllocator.Distribute(
			[Lot(1, 10, 2, 100m)],
			[Line(0, 99, 1)]);

		act.Should().Throw<InvalidOperationException>();
	}

	[Fact]
	public void Distribute_WhenLotCountIsNotPositive_Throws()
	{
		var act = () => SaleContentAllocator.Distribute(
			[Lot(1, 10, 0, 100m)],
			[Line(0, 10, 1)]);

		act.Should().Throw<InvalidOperationException>();
	}

	[Fact]
	public void CalculateTotal_SumsDiscountedLineTotals()
	{
		var contents = SaleContentAllocator.Distribute(
			[Lot(1, 10, 3, 50m), Lot(2, 20, 2, 70m)],
			[Line(0, 10, 3, 90m, 100m), Line(1, 20, 2, 40m, 40m)]);
		var sale = DomainSale.Create(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			1,
			"WH-1",
			PurchaseDate);
		foreach (var content in contents)
			sale.AddContent(content);

		sale.CalculateTotal().Should().Be(3 * 90m + 2 * 40m);
	}

	private static AllocationLot Lot(int id, int productId, int count, decimal buyPrice)
		=> new(id, productId, 1, buyPrice, count, PurchaseDate);

	private static AllocationLine Line(
		int index,
		int productId,
		int count,
		decimal discounted = 180m,
		decimal original = 200m)
		=> new(index, productId, original, discounted, count, null);
}
