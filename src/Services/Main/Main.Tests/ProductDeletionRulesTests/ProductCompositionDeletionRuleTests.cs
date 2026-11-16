using Main.Entities.Product;
using Main.Entities.Product.Product;
using Main.Modules.Product.Application.Services;
using Tests.TestContainers.Combined;

namespace Tests.ProductDeletionRulesTests;

public class ProductCompositionDeletionRuleTests : ProductDeletionRuleTestsBase<ProductCompositionDeletionRule>
{
	public ProductCompositionDeletionRuleTests(CombinedContainerFixture fixture) : base(fixture)
	{
	}

	[Fact]
	public async Task ProductUsedAsComponent_IsBlocked()
	{
		var (parent, child) = await CreateComposition();

		await ShouldBlock(child.Id, ArticleDeletionCompositionMessage.Key);
	}

	[Fact]
	public async Task ProductThatOnlyContainsOthers_IsNotBlocked()
	{
		var (parent, _) = await CreateComposition();

		await ShouldNotBlock(parent.Id);
	}

	private async Task<(Product Parent, Product Child)> CreateComposition()
	{
		var parent = await CreateUnreferencedProduct();
		var child = await CreateUnreferencedProduct();
		await Context.AddAsync(ProductContent.Create(parent.Id, child.Id, 1), CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		return (parent, child);
	}
}
