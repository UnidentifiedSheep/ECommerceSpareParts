using Application.Common.Models;
using FluentAssertions;
using Main.Application.Handlers.Purchases.GetPurchases;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Purchase;

namespace Tests.HandlersTests.Purchases;

public sealed class GetPurchasesTests : IntegrationTest
{
	public GetPurchasesTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<PurchaseTestContext>();
	}

	private PurchaseTestContext TestContext => GetContext<PurchaseTestContext>();

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
				TestContext.Purchase.SetComment("Prefix " + term + " suffix");
				break;
			case "ContentComment":
				TestContext.Purchase.Contents.First().SetComment("Prefix " + term + " suffix");
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

		result.Purchases.Select(document => document.Id).Should().Equal(TestContext.Purchase.Id);
	}

	[Theory]
	[InlineData("literal%token", "literalANYtoken")]
	[InlineData("literal_token", "literalXtoken")]
	[InlineData(@"literal\token", "literaltoken")]
	public async Task Search_TreatsLikeMetacharactersAsLiteralText(string term, string nonMatching)
	{
		TestContext.Product.SetName("Unrelated product");
		TestContext.Product.SetSku("UNRELATED-SKU");
		TestContext.Purchase.SetComment(term);
		await Context.SaveChangesAsync(CancellationToken);

		var matching = await Search(term);
		matching.Purchases.Select(document => document.Id).Should().Equal(TestContext.Purchase.Id);

		TestContext.Purchase.SetComment(nonMatching);
		await Context.SaveChangesAsync(CancellationToken);

		(await Search(term)).Purchases.Should().BeEmpty();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task Search_WithoutTerm_DoesNotFilterDocuments(string? term)
	{
		var result = await Search(term);

		result.Purchases.Select(document => document.Id).Should().Equal(TestContext.Purchase.Id);
	}

	[Theory]
	[InlineData("---")]
	[InlineData("%%%")]
	public async Task Search_WithoutSkuCharacters_DoesNotMatchEveryDocument(string term)
	{
		TestContext.Product.SetName("Unrelated product");
		TestContext.Product.SetSku("UNRELATED-SKU");
		await Context.SaveChangesAsync(CancellationToken);

		(await Search(term)).Purchases.Should().BeEmpty();
	}

	[Fact]
	public async Task Search_UnknownTerm_ReturnsNoDocuments()
	{
		(await Search("NoSuchSearchMarker")).Purchases.Should().BeEmpty();
	}

	[Fact]
	public async Task Search_StillAppliesOrganizationFilter()
	{
		TestContext.Purchase.SetComment("SearchFilterMarker");
		await Context.SaveChangesAsync(CancellationToken);
		var query = Query("searchfiltermarker") with { SupplierOrganizationIds = [Guid.NewGuid()] };

		var result = await Mediator.Send(query, CancellationToken);

		result.Purchases.Should().BeEmpty();
	}

	private Task<GetPurchasesResult> Search(string? term) => Mediator.Send(Query(term), CancellationToken);

	private static GetPurchasesQuery Query(string? term) =>
		new(new RangeModel<DateTime>(), new Pagination(0, 25), [], [], [], [], term);
}
