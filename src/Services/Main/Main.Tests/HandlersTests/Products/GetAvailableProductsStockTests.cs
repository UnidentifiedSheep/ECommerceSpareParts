using FluentAssertions;
using Main.Application.Handlers.Products;
using Main.Application.Products.Queries;
using Main.Entities.Storage;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders.Storage;
using Tests.Extensions;
using Tests.Stubs;
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

		var result = await Mediator.Send(new GetAvailableProductsStockQuery(item), CancellationToken);

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

		var result = await Mediator.Send(new GetAvailableProductsStockQuery(items), CancellationToken);

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

		var result = await Mediator.Send(new GetAvailableProductsStockQuery(item), CancellationToken);

		result.Stocks.Should().ContainSingle();
		result.Stocks[item].Should().Be(0);
	}

	[Fact]
	public async Task GetAvailableProductsStock_WithDuplicateItems_ReturnsSingleStock()
	{
		var content = TestContext.StorageContents.First(x => x.Count > 0);
		var item = new GetAvailableProductsStockItem(content.ProductId, content.StorageCode);

		var result = await Mediator.Send(new GetAvailableProductsStockQuery([item, item]), CancellationToken);

		result.Stocks.Should().ContainSingle();
		result.Stocks.Should().ContainKey(item);
	}
	[Fact]
	public async Task GetAvailableProductsStock_OverlockedPartyDoesNotReduceOtherPartiesAvailability()
	{
		var content = TestContext.StorageContents.First(item => item.Count > 0);
		var item = new GetAvailableProductsStockItem(content.ProductId, content.StorageCode);
		var expected = TestContext.StorageContents.Where(source => source.ProductId == item.ProductId &&
			source.StorageCode == item.StorageCode).Sum(source => source.Count) - content.Count + 7;
		await new StorageContentBuilder(Faker).WithProductIds(content.ProductId).WithStorageCode(content.StorageCode)
			.WithCurrencyId(content.CurrencyId).WithCount(7).BuildAndAddToDb(Context, CancellationToken);
		await Context.AddAsync(StorageContentLock.Create(content.Id, content.Count + 3), CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(new GetAvailableProductsStockQuery(item), CancellationToken);

		result.Stocks[item].Should().Be(expected);
	}

	[Fact]
	public async Task GetAvailableProductsStock_OnlyActiveUnexpiredLocksReduceAvailability()
	{
		var content = TestContext.StorageContents.First(source => source.Count > 0);
		var item = new GetAvailableProductsStockItem(content.ProductId, content.StorageCode);
		var expected = TestContext.StorageContents.Where(source => source.ProductId == item.ProductId &&
			source.StorageCode == item.StorageCode).Sum(source => source.Count) - 1;
		var clock = Scope.ServiceProvider.GetRequiredService<TestTimeProvider>();
		var now = clock.GetUtcNow().UtcDateTime;
		var released = StorageContentLock.Create(content.Id, content.Count);
		released.Release(now);
		await Context.AddRangeAsync([
			released,
			StorageContentLock.Create(content.Id, content.Count, now.AddHours(1)),
			StorageContentLock.Create(content.Id, 1, now.AddHours(2))
		], CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		clock.SetUtcNow(now.AddHours(1));

		var result = await Mediator.Send(new GetAvailableProductsStockQuery(item), CancellationToken);

		result.Stocks[item].Should().Be(expected);
	}

}
