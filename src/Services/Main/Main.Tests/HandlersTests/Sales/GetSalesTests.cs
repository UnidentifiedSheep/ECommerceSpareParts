using Application.Common.Models;
using FluentAssertions;
using Main.Application.Handlers.Sales.GetSales;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Sale;

namespace Tests.HandlersTests.Sales;

public sealed class GetSalesTests : IntegrationTest
{
	public GetSalesTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<SaleTestContext>();
	}

	private SaleTestContext TestContext => GetContext<SaleTestContext>();

	[Theory]
	[InlineData("DocumentComment")]
	[InlineData("ContentComment")]
	[InlineData("ProductName")]
	[InlineData("Sku")]
	[InlineData("FormattedSku")]
	public async Task Search_MatchesTextFields(string field)
	{
		var term = "RegressionMarker";
		switch (field)
		{
			case "DocumentComment":
				TestContext.Sale.SetComment("Prefix " + term + " suffix");
				break;
			case "ContentComment":
				TestContext.Sale.Contents[0].SetComment("Prefix " + term + " suffix");
				break;
			case "ProductName":
				TestContext.Product.SetName("Prefix " + term + " suffix");
				break;
			case "Sku":
				TestContext.Product.SetSku("PREFIX-REGRESSIONMARKER-SUFFIX");
				break;
			case "FormattedSku":
				TestContext.Product.SetSku("PREFIX-REGRESSION-MARKER-SUFFIX");
				term = "Regression-Marker";
				break;
		}
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Search($" {term.ToLowerInvariant()} ");

		result.Sales.Select(document => document.Id).Should().Equal(TestContext.Sale.Id);
	}

	[Fact]
	public async Task Search_ByBuyerUserName_ExecutesInDatabase()
	{
		var result = await Search($" {TestContext.Buyer.UserName.Value.ToLowerInvariant()} ");

		result.Sales.Select(sale => sale.Id).Should().Equal(TestContext.Sale.Id);
	}

	[Theory]
	[InlineData("literal%token", "literalANYtoken")]
	[InlineData("literal_token", "literalXtoken")]
	[InlineData(@"literal\token", "literaltoken")]
	public async Task Search_TreatsLikeMetacharactersAsLiteralText(string term, string nonMatching)
	{
		TestContext.Product.SetName("Unrelated product");
		TestContext.Product.SetSku("UNRELATED-SKU");
		TestContext.Sale.SetComment(term);
		await Context.SaveChangesAsync(CancellationToken);

		var matching = await Search(term);
		matching.Sales.Select(document => document.Id).Should().Equal(TestContext.Sale.Id);

		TestContext.Sale.SetComment(nonMatching);
		await Context.SaveChangesAsync(CancellationToken);

		(await Search(term)).Sales.Should().BeEmpty();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task Search_WithoutTerm_DoesNotFilterDocuments(string? term)
	{
		var result = await Search(term);

		result.Sales.Select(document => document.Id).Should().Equal(TestContext.Sale.Id);
	}

	[Theory]
	[InlineData("---")]
	[InlineData("%%%")]
	public async Task Search_WithoutSkuCharacters_DoesNotMatchEveryDocument(string term)
	{
		TestContext.Product.SetName("Unrelated product");
		TestContext.Product.SetSku("UNRELATED-SKU");
		await Context.SaveChangesAsync(CancellationToken);

		(await Search(term)).Sales.Should().BeEmpty();
	}

	[Fact]
	public async Task Search_UnknownTerm_ReturnsNoDocuments()
	{
		(await Search("NoSuchSearchMarker")).Sales.Should().BeEmpty();
	}

	[Fact]
	public async Task Search_StillAppliesOrganizationFilter()
	{
		TestContext.Sale.SetComment("SearchFilterMarker");
		await Context.SaveChangesAsync(CancellationToken);
		var query = Query("searchfiltermarker") with { OrganizationIds = [Guid.NewGuid()] };

		var result = await Mediator.Send(query, CancellationToken);

		result.Sales.Should().BeEmpty();
	}

	private Task<GetSalesResult> Search(string? term) => Mediator.Send(Query(term), CancellationToken);

	private static GetSalesQuery Query(string? term) =>
		new(new RangeModel<DateTime>(), new Pagination(0, 25), [], [], [], [], [], term);
}
