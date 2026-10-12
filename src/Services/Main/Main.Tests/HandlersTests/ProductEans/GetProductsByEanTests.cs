using Application.Common.Models;
using FluentAssertions;
using Main.Application.Handlers.ProductEans.GetProductsByEan;
using Main.Entities.Product;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.ProductEans;

public class GetProductsByEanTests : IntegrationTest
{
	public GetProductsByEanTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<ProductEanTestContext>();

	private ProductEanTestContext TestContext => GetContext<ProductEanTestContext>();

	[Fact]
	public async Task GetProductsByEan_ReturnsMatchingProductsInPages()
	{
		var existing = TestContext.ProductEans.First();
		var otherProducts = TestContext.ProductTestContext.Products.Skip(3).Take(2).ToArray();
		Context.ProductEans.AddRange(otherProducts.Select(x => ProductEan.Create(x.Id, existing.Ean)));
		await Context.SaveChangesAsync(CancellationToken);

		var expectedIds = otherProducts.Select(x => x.Id).Append(existing.ProductId).Order().ToArray();
		var firstPage = await Mediator.Send(
			new GetProductsByEanQuery($" {existing.Ean} ", new Pagination(0, 2)),
			CancellationToken);
		var secondPage = await Mediator.Send(
			new GetProductsByEanQuery(existing.Ean, new Pagination(1, 2)),
			CancellationToken);

		firstPage.Products.Select(x => x.Id).Should().Equal(expectedIds.Take(2));
		secondPage.Products.Select(x => x.Id).Should().Equal(expectedIds.Skip(2));
	}

	[Fact]
	public async Task GetProductsByEan_UnknownEan_ReturnsEmptyList()
	{
		var result = await Mediator.Send(
			new GetProductsByEanQuery("9999999999999", new Pagination(0, 10)),
			CancellationToken);

		result.Products.Should().BeEmpty();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("123456789012345")]
	public async Task GetProductsByEan_InvalidEan_ThrowsValidationException(string? ean)
	{
		var action = () => Mediator.Send(
			new GetProductsByEanQuery(ean!, new Pagination(0, 10)),
			CancellationToken);

		var exception = await Assert.ThrowsAsync<ValidationException>(action);
		exception.Errors.Should().ContainSingle(x => x.PropertyName == nameof(GetProductsByEanQuery.Ean));
	}
}
