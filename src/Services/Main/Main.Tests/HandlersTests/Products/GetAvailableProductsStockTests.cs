using FluentAssertions;
using Main.Application.Handlers.Products;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Products;

public class GetAvailableProductsStockTests : IntegrationTest
{
	public GetAvailableProductsStockTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<StorageContentTestContext>();
	}

	private StorageContentTestContext TestContext => GetContext<StorageContentTestContext>();

	[Fact]
	public async Task GetAvailableProductsStock_WithSingleItem_ReturnsStock()
	{
		var content = TestContext.StorageContents.First(x => x.Count > 0);
		var item = new GetAvailableProductsStockItem(content.ProductId, content.StorageCode);
		var expected = TestContext
			.StorageContents
			.Where(x => x.ProductId == item.ProductId && x.StorageCode == item.StorageCode)
			.Sum(x => x.Count);

		var result = await Mediator.Send(new GetAvailableProductsStockQuery(item));

		result.Stocks.Should().ContainSingle();
		result.Stocks[item].Should().Be(expected);
	}

	[Fact]
	public async Task GetAvailableProductsStock_WithMultipleItems_ReturnsStockForEveryItem()
	{
		var items = TestContext
			.StorageContents
			.Select(x => new GetAvailableProductsStockItem(x.ProductId, x.StorageCode))
			.Distinct()
			.Take(3)
			.ToArray();

		var result = await Mediator.Send(new GetAvailableProductsStockQuery(items));

		result.Stocks.Should().HaveCount(items.Length);

		foreach (var item in items)
		{
			var expected = TestContext
				.StorageContents
				.Where(x => x.ProductId == item.ProductId && x.StorageCode == item.StorageCode)
				.Sum(x => x.Count);

			result.Stocks[item].Should().Be(expected);
		}
	}

	[Fact]
	public async Task GetAvailableProductsStock_WithUnknownItem_ReturnsZero()
	{
		var content = TestContext.StorageContents.First(x => x.Count > 0);
		var item = new GetAvailableProductsStockItem(content.ProductId, "unknown-storage");

		var result = await Mediator.Send(new GetAvailableProductsStockQuery(item));

		result.Stocks.Should().ContainSingle();
		result.Stocks[item].Should().Be(0);
	}

	[Fact]
	public async Task GetAvailableProductsStock_WithDuplicateItems_ReturnsSingleStock()
	{
		var content = TestContext.StorageContents.First(x => x.Count > 0);
		var item = new GetAvailableProductsStockItem(content.ProductId, content.StorageCode);

		var result = await Mediator.Send(new GetAvailableProductsStockQuery([item, item]));

		result.Stocks.Should().ContainSingle();
		result.Stocks.Should().ContainKey(item);
	}
}
