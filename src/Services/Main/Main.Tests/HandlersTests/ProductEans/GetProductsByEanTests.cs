using Application.Common.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Main.Entities.Product.Product;
using Main.Modules.Product.Application.Queries.Products.GetProductsByEan;
using Tests.Abstractions.Test;
using Tests.TestContexts;

namespace Tests.HandlersTests.ProductEans;

public class GetProductsByEanTests(ReadScenarioFixture<GetProductsByEanTests.ProductsByEanScenario> fixture)
	: ReadScenarioTest<GetProductsByEanTests.ProductsByEanScenario>(fixture)
{
	[Fact]
	public async Task GetProductsByEan_ReturnsMatchingProductsInPages()
	{
		var ean = Scenario.SharedEan;

		var firstPage = await Mediator.Send(
			new GetProductsByEanQuery($" {ean} ", new Pagination(0, 2)),
			CancellationToken);
		var secondPage = await Mediator.Send(
			new GetProductsByEanQuery(ean, new Pagination(1, 2)),
			CancellationToken);

		firstPage.Products.Select(x => x.Id).Should().Equal(Scenario.SharedProductIds.Take(2));
		secondPage.Products.Select(x => x.Id).Should().Equal(Scenario.SharedProductIds.Skip(2));
	}

	[Fact]
	public async Task GetProductsByEan_UnknownEan_ReturnsEmptyList()
	{
		var result = await Mediator.Send(
			new GetProductsByEanQuery("9999999999999", new Pagination(0, 10)),
			CancellationToken);

		result.Products.Should().BeEmpty();
	}

	public sealed class ProductsByEanScenario : ReadScenario
	{
		public ProductsByEanScenario() => RegisterBasicContext<ProductEanTestContext>();

		public string SharedEan { get; private set; } = null!;

		public int[] SharedProductIds { get; private set; } = [];

		protected override IEnumerable<object> Seed()
		{
			var context = Get<ProductEanTestContext>();
			var existing = context.ProductEans.First();
			var otherProducts = context.ProductTestContext.Products.Skip(3).Take(2).ToArray();
			SharedEan = existing.Ean;
			SharedProductIds = otherProducts.Select(x => x.Id).Append(existing.ProductId).Order().ToArray();
			return otherProducts.Select(x => ProductEan.Create(x.Id, existing.Ean));
		}
	}
}

public class
	GetProductsByEanValidationTests : ValidationTest<GetProductsByEanValidation, GetProductsByEanQuery>
{
	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("123456789012345")]
	public async Task GetProductsByEan_InvalidEan_ThrowsValidationException(string? ean)
	{
		var result = await ValidateAsync(new GetProductsByEanQuery(ean!, new Pagination(0, 10)));
		result.ShouldHaveValidationErrorFor(x => x.Ean).Only();
	}
}
