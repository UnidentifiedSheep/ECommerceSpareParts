using FluentAssertions;
using Main.Application.Handlers.ProductEans.GetProductsEans;
using Main.Entities.Product;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.ProductEans;

public class GetProductsEansTests : IntegrationTest
{
	public GetProductsEansTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<ProductEanTestContext>();

	private ProductEanTestContext TestContext => GetContext<ProductEanTestContext>();

	[Fact]
	public async Task GetProductsEans_GroupsAndOrdersEansByProduct()
	{
		var productIds = TestContext.ProductTestContext.Products.Take(4).Select(x => x.Id).ToArray();
		const string extraEan = "9999999999999";
		Context.ProductEans.Add(ProductEan.Create(productIds[0], extraEan));
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(
			new GetProductsEansQuery([productIds[0], productIds[1], productIds[3], productIds[0]]),
			CancellationToken);

		result.Eans.Keys.Should().BeEquivalentTo(productIds.Take(2));
		result.Eans.Should().NotContainKey(productIds[3]);
		result.Eans[productIds[0]].Should().Equal(
			TestContext.ProductEans.Where(x => x.ProductId == productIds[0])
				.Select(x => x.Ean).Append(extraEan).Order());
		result.Eans[productIds[1]].Should().Equal(
			TestContext.ProductEans.Where(x => x.ProductId == productIds[1])
				.Select(x => x.Ean).Order());
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

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task GetProductsEans_InvalidProductId_ThrowsValidationException(int productId)
	{
		var action = () => Mediator.Send(
			new GetProductsEansQuery([TestContext.ProductTestContext.Products[0].Id, productId]),
			CancellationToken);

		var exception = await Assert.ThrowsAsync<ValidationException>(action);
		exception.Errors.Should().ContainSingle(x => x.PropertyName == "ProductIds[1]");
	}
}
