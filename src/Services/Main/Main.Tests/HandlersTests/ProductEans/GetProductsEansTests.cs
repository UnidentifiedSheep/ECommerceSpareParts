using FluentAssertions;
using FluentValidation.TestHelper;
using Main.Entities.Product.Product;
using Main.Modules.Product.Application.Queries.Products.GetProductsEans;
using Tests.Abstractions.Test;
using Tests.TestContexts;

namespace Tests.HandlersTests.ProductEans;

public class GetProductsEansTests(ReadScenarioFixture<GetProductsEansTests.ProductsEansScenario> fixture)
	: ReadScenarioTest<GetProductsEansTests.ProductsEansScenario>(fixture)
{
	private ProductEanTestContext TestContext => Scenario.Get<ProductEanTestContext>();

	[Fact]
	public async Task GetProductsEans_GroupsAndOrdersEansByProduct()
	{
		var productIds = TestContext.ProductTestContext.Products.Take(4).Select(x => x.Id).ToArray();

		var result = await Mediator.Send(
			new GetProductsEansQuery([productIds[0], productIds[1], productIds[3], productIds[0]]),
			CancellationToken);

		result.Eans.Keys.Should().BeEquivalentTo(productIds.Take(2));
		result.Eans.Should().NotContainKey(productIds[3]);
		result.Eans[productIds[0]]
			.Should()
			.Equal(
				TestContext.ProductEans
					.Where(x => x.ProductId == productIds[0])
					.Select(x => x.Ean)
					.Append(ProductsEansScenario.ExtraEan)
					.Order());
		result.Eans[productIds[1]]
			.Should()
			.Equal(
				TestContext.ProductEans
					.Where(x => x.ProductId == productIds[1])
					.Select(x => x.Ean)
					.Order());
	}

	[Fact]
	public async Task GetProductsEans_EmptyIds_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetProductsEansQuery([]), CancellationToken);

		result.Eans.Should().BeEmpty();
	}

	[Fact]
	public async Task GetProductsEans_UnknownProductId_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetProductsEansQuery([int.MaxValue]), CancellationToken);

		result.Eans.Should().BeEmpty();
	}

	public sealed class ProductsEansScenario : ReadScenario
	{
		public const string ExtraEan = "9999999999999";

		public ProductsEansScenario() => RegisterBasicContext<ProductEanTestContext>();

		protected override IEnumerable<object> Seed()
			=> [ProductEan.Create(Get<ProductEanTestContext>().ProductTestContext.Products.First().Id, ExtraEan)];
	}
}

public class GetProductsEansValidationTests : ValidationTest<GetProductsEansValidation, GetProductsEansQuery>
{
	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task GetProductsEans_InvalidProductId_ThrowsValidationException(int productId)
	{
		var result = await ValidateAsync(new GetProductsEansQuery([1, productId]));
		result.ShouldHaveValidationErrorFor("ProductIds[1]").Only();
	}
}
