using Application.Common.Models;
using Contracts.Products;
using Contracts.Storage;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Search.Application.Consumers.Product;
using Search.Application.Models.CatalogueSearch;
using Search.Application.Models.ProductUpdates;
using Search.Enums;
using Search.Persistence.Models;
using Tests.TestContainers.Search;

namespace Search.Tests.Consumers;

public sealed class ProductStocksSnapshotConsumerTests(SearchContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task FullSnapshot_ReplacesOldWarehousesAndEmptySnapshotClearsStock()
	{
		var now = DateTime.UtcNow;
		await Snapshot(901, now, new Dictionary<string, int> { ["warehouse-a"] = 5, ["warehouse-b"] = 7 });
		await Snapshot(901, now.AddSeconds(1), new Dictionary<string, int> { ["warehouse-b"] = 3 });

		var product = (await Products.GetById(901, CancellationToken))!;
		product.Stocks.Should().BeEquivalentTo(new Dictionary<string, int> { ["warehouse-b"] = 3 });
		product.Stock.Should().Be(3L);

		await Snapshot(901, now.AddSeconds(2), new Dictionary<string, int>());

		product = (await Products.GetById(901, CancellationToken))!;
		product.Stocks.Should().BeEmpty();
		product.Stock.Should().Be(0L);
	}

	[Fact]
	public async Task DelayedSnapshot_PreservesNewerWarehouseValuesAndZeroRemovals()
	{
		var now = DateTime.UtcNow;
		await Snapshot(902, now, new Dictionary<string, int>
		{
			["warehouse-a"] = 10, ["warehouse-b"] = 20, ["removed-warehouse"] = 40
		});
		await Update(902, "warehouse-a", 7, now.AddSeconds(20));
		await Update(902, "warehouse-b", 0, now.AddSeconds(30));

		await Snapshot(902, now.AddSeconds(10), new Dictionary<string, int>
		{
			["warehouse-a"] = 100, ["warehouse-b"] = 100, ["warehouse-c"] = 3
		});
		await Update(902, "warehouse-b", 50, now.AddSeconds(25));
		await Snapshot(902, now.AddSeconds(5), new Dictionary<string, int> { ["removed-warehouse"] = 999 });

		var product = (await Products.GetById(902, CancellationToken))!;
		product.Stocks.Should().BeEquivalentTo(new Dictionary<string, int> { ["warehouse-a"] = 7, ["warehouse-c"] = 3 });
		product.Stock.Should().Be(10L);
		product.StockSnapshotVersion.Should().Be(now.AddSeconds(10).Ticks);

		await Snapshot(902, now.AddSeconds(40), new Dictionary<string, int>());
		await Update(902, "warehouse-a", 25, now.AddSeconds(35));

		product = (await Products.GetById(902, CancellationToken))!;
		product.Stocks.Should().BeEmpty();
		product.StockVersions.Should().BeEmpty();
		product.Stock.Should().Be(0L);
	}

	[Fact]
	public async Task BatchSnapshots_KeepProductsIndependentAndUseLongForTotal()
	{
		var notification = new ProductStocksSnapshotEvent
		{
			OccurredAt = DateTime.UtcNow,
			Products =
			[
				new ProductStocksSnapshot
				{
					ProductId = 903,
					Stocks = new Dictionary<string, int> { ["warehouse-a"] = int.MaxValue, ["warehouse-b"] = int.MaxValue }
				},
				new ProductStocksSnapshot { ProductId = 904, Stocks = new Dictionary<string, int>() }
			]
		};

		await Scope.ServiceProvider.GetRequiredService<ProductStocksSnapshotConsumer>().Consume(Context(notification));

		(await Products.GetById(903, CancellationToken))!.Stock.Should().Be(2L * int.MaxValue);
		(await Products.GetById(904, CancellationToken))!.Stock.Should().Be(0L);
	}

	[Fact]
	public async Task ArbitraryStorageCodes_DoNotConflictAsObjectPaths()
	{
		var codes = new Dictionary<string, int> { ["warehouse"] = 2, ["warehouse.a"] = 3, ["warehouse%2Ea"] = 4 };

		await Snapshot(905, DateTime.UtcNow, codes);

		var product = (await Products.GetById(905, CancellationToken))!;
		product.Stocks.Should().BeEquivalentTo(codes.ToDictionary(pair => ProductStockFields.GetStorageKey(pair.Key), pair => pair.Value));
		product.Stock.Should().Be(9L);
	}

	[Fact]
	public async Task MoreThanThousandWarehouses_FitProductMapping()
	{
		var stocks = Enumerable.Range(0, 1100).ToDictionary(number => $"warehouse-{number}", _ => 1);

		await Snapshot(906, DateTime.UtcNow, stocks);

		var product = (await Products.GetById(906, CancellationToken))!;
		product.Stocks.Should().HaveCount(1100);
		product.Stock.Should().Be(1100L);
	}

	[Fact]
	public async Task Catalogue_SortsProductsByTotalStock()
	{
		var now = DateTime.UtcNow;
		await Products.ApplyUpdates(new[] { 901, 902, 903 }.Select(id => ProductCoreUpdate.From(
			new ProductSnapshot { Id = id, Sku = $"STOCK-{id}", Name = "Stock product", ProducerId = 1 }, now)).ToArray(), CancellationToken);
		await Snapshot(901, now, new Dictionary<string, int> { ["warehouse-a"] = 10 });
		await Snapshot(902, now, new Dictionary<string, int>());
		await Snapshot(903, now, new Dictionary<string, int> { ["warehouse-a"] = 5 });
		await RefreshAsync();

		var result = await Products.Search(new CatalogueSearchCriteria
		{
			Query = "", SkuModes = new HashSet<SearchMatchType>(), NameModes = new HashSet<SearchMatchType>(),
			Pagination = new Pagination(0, 20), SortBy = ["stock"]
		}, CancellationToken);

		result.Hits.Select(item => item.Document.Id).Should().Equal(902, 903, 901);
	}

	private Task Snapshot(int id, DateTime occurredAt, IReadOnlyDictionary<string, int> stocks) =>
		Scope.ServiceProvider.GetRequiredService<ProductStocksSnapshotConsumer>().Consume(Context(new ProductStocksSnapshotEvent
		{
			OccurredAt = occurredAt,
			Products = [new ProductStocksSnapshot { ProductId = id, Stocks = stocks }]
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
