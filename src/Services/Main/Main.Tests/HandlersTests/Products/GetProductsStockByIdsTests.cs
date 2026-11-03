using FluentAssertions;
using Main.Application.Handlers.Products;
using Main.Entities.Storage;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Products;

public class GetProductsStockByIdsTests : IntegrationTest
{
	public GetProductsStockByIdsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	[Fact]
	public async Task MultipleProductsAndStorages_ReturnIndependentTotalsForRequestedProducts()
	{
		var products = GetContext<ProductTestContext>().Products;
		var storages = GetContext<StorageTestContext>().Storages.ToArray();
		await Context.AddRangeAsync([
			ProductStock.Create(storages[0].Code, products[0].Id, 5),
			ProductStock.Create(storages[1].Code, products[0].Id, 7),
			ProductStock.Create(storages[0].Code, products[1].Id, 3),
			ProductStock.Create(storages[1].Code, products[2].Id, 100)
		], CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(new GetProductsStockByIdsQuery([products[0].Id, products[1].Id]), CancellationToken);

		result.Stocks.Should().BeEquivalentTo(new Dictionary<int, long>
		{
			[products[0].Id] = 12,
			[products[1].Id] = 3
		});
	}

	[Fact]
	public async Task MissingStockAndUnknownProduct_ReturnZero()
	{
		var product = GetContext<ProductTestContext>().Products[0];

		var result = await Mediator.Send(new GetProductsStockByIdsQuery([product.Id, int.MaxValue]), CancellationToken);

		result.Stocks.Should().BeEquivalentTo(new Dictionary<int, long> { [product.Id] = 0, [int.MaxValue] = 0 });
	}

	[Fact]
	public async Task DuplicateIds_ReturnOneTotalPerProduct()
	{
		var product = GetContext<ProductTestContext>().Products[0];
		var storage = GetContext<StorageTestContext>().Storages.First();
		await Context.AddAsync(ProductStock.Create(storage.Code, product.Id, 9), CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(new GetProductsStockByIdsQuery([product.Id, product.Id]), CancellationToken);

		result.Stocks.Should().ContainSingle().Which.Should().Be(new KeyValuePair<int, long>(product.Id, 9));
	}

	[Fact]
	public async Task TotalExceedsIntegerMaximum_ReturnsLongQuantity()
	{
		var product = GetContext<ProductTestContext>().Products[0];
		var storages = GetContext<StorageTestContext>().Storages.ToArray();
		await Context.AddRangeAsync([
			ProductStock.Create(storages[0].Code, product.Id, int.MaxValue),
			ProductStock.Create(storages[1].Code, product.Id, int.MaxValue)
		], CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(new GetProductsStockByIdsQuery(product.Id), CancellationToken);

		result.Stocks[product.Id].Should().Be(2L * int.MaxValue);
	}

	[Fact]
	public async Task EmptyIds_ReturnEmptyStocks()
	{
		var result = await Mediator.Send(new GetProductsStockByIdsQuery([]), CancellationToken);

		result.Stocks.Should().BeEmpty();
	}
}
