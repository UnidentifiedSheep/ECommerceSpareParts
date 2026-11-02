using FluentAssertions;
using Main.Entities.Storage;

namespace Tests.Domain.Storage;

public class ProductStockTests
{
	[Fact]
	public void Create_StoresQuantityAndCompositeIdentity()
	{
		var stock = ProductStock.Create(" warehouse-1 ", 42, 12);

		stock.StorageCode.Should().Be("warehouse-1");
		stock.ProductId.Should().Be(42);
		stock.Quantity.Should().Be(12);
		stock.GetId().Should().Be(("warehouse-1", 42));
		ProductStock.GetEqualityExpression(("warehouse-1", 42)).Compile()(stock).Should().BeTrue();
		ProductStock.GetEqualityExpression(("warehouse-2", 42)).Compile()(stock).Should().BeFalse();
		ProductStock.GetEqualityExpression(("warehouse-1", 43)).Compile()(stock).Should().BeFalse();
	}

	[Fact]
	public void SetQuantity_AllowsZeroAndMaximumInteger()
	{
		var stock = ProductStock.Create("warehouse-1", 42);
		stock.Quantity.Should().Be(0);

		stock.SetQuantity(int.MaxValue);
		stock.Quantity.Should().Be(int.MaxValue);
		stock.SetQuantity(0);
		stock.Quantity.Should().Be(0);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(int.MinValue)]
	public void InvalidQuantity_IsRejectedWithoutChangingStock(int quantity)
	{
		var stock = ProductStock.Create("warehouse-1", 42, 5);

		Assert.Throws<InvalidOperationException>(() => stock.SetQuantity(quantity));
		Assert.Throws<InvalidOperationException>(() => ProductStock.Create("warehouse-1", 42, quantity));
		stock.Quantity.Should().Be(5);
	}

	[Theory]
	[InlineData(null, 42)]
	[InlineData("", 42)]
	[InlineData("  ", 42)]
	[InlineData("warehouse-1", 0)]
	[InlineData("warehouse-1", -1)]
	public void Create_RejectsInvalidIdentity(string? storageCode, int productId) =>
		Assert.Throws<InvalidOperationException>(() => ProductStock.Create(storageCode!, productId));
}
