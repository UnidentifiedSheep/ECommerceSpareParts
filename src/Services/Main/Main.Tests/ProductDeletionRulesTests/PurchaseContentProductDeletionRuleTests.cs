using Main.Application.Services.ProductDeletion;
using Main.Entities.Product;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Purchase;

namespace Tests.ProductDeletionRulesTests;

public class PurchaseContentProductDeletionRuleTests : ProductDeletionRuleTestsBase<PurchaseContentProductDeletionRule>
{
	public PurchaseContentProductDeletionRuleTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<PurchaseTestContext>();

	[Fact]
	public async Task ProductInPurchase_IsBlocked()
		=> await ShouldBlock(GetContext<PurchaseTestContext>().Product.Id, ArticleDeletionPurchaseMessage.Key);

	[Fact]
	public async Task ProductNotInPurchase_IsNotBlocked()
		=> await ShouldNotBlock((await CreateUnreferencedProduct()).Id);
}
