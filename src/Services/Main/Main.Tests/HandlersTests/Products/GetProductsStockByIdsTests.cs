using FluentAssertions;
using Main.Entities.Product.Product;
using Main.Entities.Product.Stock;
using Main.Modules.Product.Application.Queries.Products;
using Tests.TestContexts;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Products;

public class GetProductsStockByIdsTests(ReadScenarioFixture<GetProductsStockByIdsTests.ProductsStockScenario> fixture)
	: ReadScenarioTest<GetProductsStockByIdsTests.ProductsStockScenario>(fixture)
{
	private IReadOnlyList<Product> Products => Scenario.Get<ProductTestContext>().Products;

	[Fact]
	public async Task MultipleProductsAndStorages_ReturnIndependentTotalsForRequestedProducts()
	{
		var result = await Mediator.Send(
			new GetProductsStockByIdsQuery([Products[0].Id, Products[1].Id]),
			CancellationToken);

		result.Stocks
			.Should()
			.BeEquivalentTo(
				new Dictionary<int, long>
				{
					[Products[0].Id] = 12,
					[Products[1].Id] = 3
				});
	}

	[Fact]
	public async Task MissingStockAndUnknownProduct_ReturnZero()
	{
		var product = Products[4];

		var result = await Mediator.Send(
			new GetProductsStockByIdsQuery([product.Id, int.MaxValue]),
			CancellationToken);

		result.Stocks
			.Should()
			.BeEquivalentTo(new Dictionary<int, long> { [product.Id] = 0, [int.MaxValue] = 0 });
	}

	[Fact]
	public async Task DuplicateIds_ReturnOneTotalPerProduct()
	{
		var product = Products[1];

		var result = await Mediator.Send(
			new GetProductsStockByIdsQuery([product.Id, product.Id]),
			CancellationToken);

		result.Stocks.Should().ContainSingle().Which.Should().Be(new KeyValuePair<int, long>(product.Id, 3));
	}

	[Fact]
	public async Task TotalExceedsIntegerMaximum_ReturnsLongQuantity()
	{
		var product = Products[3];

		var result = await Mediator.Send(new GetProductsStockByIdsQuery(product.Id), CancellationToken);

		result.Stocks[product.Id].Should().Be(2L * int.MaxValue);
	}

	[Fact]
	public async Task EmptyIds_ReturnEmptyStocks()
	{
		var result = await Mediator.Send(new GetProductsStockByIdsQuery([]), CancellationToken);

		result.Stocks.Should().BeEmpty();
	}

	public sealed class ProductsStockScenario : ReadScenario
	{
		public ProductsStockScenario()
		{
			RegisterBasicContext<ProductTestContext>();
			RegisterBasicContext<StorageTestContext>();
		}

		protected override IEnumerable<object> Seed()
		{
			var products = Get<ProductTestContext>().Products;
			var storages = Get<StorageTestContext>().Storages.ToArray();
			return
			[
				ProductStock.Create(storages[0].Code, products[0].Id, 5),
				ProductStock.Create(storages[1].Code, products[0].Id, 7),
				ProductStock.Create(storages[0].Code, products[1].Id, 3),
				ProductStock.Create(storages[1].Code, products[2].Id, 100),
				ProductStock.Create(storages[0].Code, products[3].Id, int.MaxValue),
				ProductStock.Create(storages[1].Code, products[3].Id, int.MaxValue)
			];
		}
	}
}
