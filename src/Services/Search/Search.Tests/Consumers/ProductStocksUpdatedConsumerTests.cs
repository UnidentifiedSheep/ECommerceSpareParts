using Contracts.Products;
using Contracts.Storage;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Search.Application.Consumers.Product;
using Search.Enums;
using Tests.TestContainers.Search;

namespace Search.Tests.Consumers;

public sealed class ProductStocksUpdatedConsumerTests(SearchContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task WarehouseUpdates_IgnoreDuplicatesAndCompareVersionsPerWarehouse()
	{
		var now = DateTime.UtcNow;
		await Snapshot(901, now);
		await Update(901, "warehouse-a", 7, now.AddSeconds(30));
		await Update(901, "warehouse-b", 4, now.AddSeconds(20));
		await Update(901, "warehouse-a", 7, now.AddSeconds(30));
		await Update(901, "warehouse-a", 99, now.AddSeconds(29));

		var product = (await Products.GetById(901, CancellationToken))!;
		product.Stocks.Should().BeEquivalentTo(new Dictionary<string, int> { ["warehouse-a"] = 7, ["warehouse-b"] = 4 });
		product.Stock.Should().Be(11L);
	}

	[Fact]
	public async Task ZeroUpdate_RemovesWarehouseAndRejectsOlderPositiveQuantity()
	{
		var now = DateTime.UtcNow;
		await Snapshot(902, now);
		await Update(902, "warehouse-a", 5, now.AddSeconds(1));
		await Update(902, "warehouse-a", 0, now.AddSeconds(3));
		await Update(902, "warehouse-a", 10, now.AddSeconds(2));

		var product = (await Products.GetById(902, CancellationToken))!;
		product.Stocks.Should().BeEmpty();
		product.Stock.Should().Be(0L);
	}

	[Fact]
	public async Task UpdateBeforeFullSnapshot_LeavesTotalUnknownAndDoesNotMakeProductReady()
	{
		var now = DateTime.UtcNow;
		await Update(903, "warehouse-a", 5, now.AddSeconds(20));

		var product = (await Products.GetById(903, CancellationToken))!;
		product.Stock.Should().BeNull();
		product.ReadinessStatus.Should().Be(ProductReadinessStatus.Partial);

		await Snapshot(903, now);

		product = (await Products.GetById(903, CancellationToken))!;
		product.Stock.Should().Be(5L);
		product.Stocks.Should().BeEquivalentTo(new Dictionary<string, int> { ["warehouse-a"] = 5 });
		product.ReadinessStatus.Should().Be(ProductReadinessStatus.Partial);
	}

	[Fact]
	public async Task ConcurrentUpdatesForDifferentWarehouses_DoNotLoseValues()
	{
		var now = DateTime.UtcNow;
		await Snapshot(904, now);

		await Task.WhenAll(Update(904, "warehouse-a", 5, now.AddSeconds(1)),
			Update(904, "warehouse-b", 7, now.AddSeconds(1)));

		var product = (await Products.GetById(904, CancellationToken))!;
		product.Stocks.Should().BeEquivalentTo(new Dictionary<string, int> { ["warehouse-a"] = 5, ["warehouse-b"] = 7 });
		product.Stock.Should().Be(12L);
	}

	[Fact]
	public async Task StockEvents_DoNotReviveDeletedProduct()
	{
		var now = DateTime.UtcNow;
		await Scope.ServiceProvider.GetRequiredService<ProductUpdatedConsumer>().Consume(Context(new ProductsUpdatedEvent
		{
			UpdatedAt = now, Products = [new ProductSnapshot { Id = 905, IsDeleted = true }]
		}));

		await Snapshot(905, now.AddSeconds(1));
		await Update(905, "warehouse-a", 5, now.AddSeconds(2));

		(await Products.GetById(905, CancellationToken))!.ReadinessStatus.Should().Be(ProductReadinessStatus.Deleted);
	}

	[Fact]
	public async Task UpdatesWithinSameMillisecond_PreserveExactVersionOrdering()
	{
		var now = new DateTime(2026, 11, 3, 12, 0, 0, DateTimeKind.Utc);
		await Snapshot(906, now);
		await Update(906, "warehouse-a", 7, now.AddTicks(2));
		await Update(906, "warehouse-a", 99, now.AddTicks(1));

		var product = (await Products.GetById(906, CancellationToken))!;
		product.Stock.Should().Be(7L);
		product.StockVersions["warehouse-a"].Should().Be(now.AddTicks(2).Ticks);
		product.StockSnapshotVersion.Should().Be(now.Ticks);
	}

	private Task Snapshot(int id, DateTime occurredAt) =>
		Scope.ServiceProvider.GetRequiredService<ProductStocksSnapshotConsumer>().Consume(Context(new ProductStocksSnapshotEvent
		{
			OccurredAt = occurredAt,
			Products = [new ProductStocksSnapshot { ProductId = id, Stocks = new Dictionary<string, int>() }]
		}));

	private Task Update(int id, string storage, int quantity, DateTime occurredAt) =>
		Scope.ServiceProvider.GetRequiredService<ProductStocksUpdatedConsumer>().Consume(Context(new ProductStocksUpdatedEvent
		{
			OccurredAt = occurredAt, StorageCode = storage,
			Items = [new ProductStockItem { ProductId = id, Quantity = quantity }]
		}));

	private ConsumeContext<T> Context<T>(T message) where T : class
	{
		var context = new Mock<ConsumeContext<T>>();
		context.SetupGet(value => value.Message).Returns(message);
		context.SetupGet(value => value.CancellationToken).Returns(CancellationToken);
		return context.Object;
	}
}
