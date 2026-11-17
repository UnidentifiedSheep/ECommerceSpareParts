using Main.Entities.Purchase;
using Main.Modules.Purchase.Application.Services.ProductDeletion;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Purchase;

namespace Tests.ProductDeletionRulesTests;

public class
	PurchaseContentProductDeletionRuleTests : ProductDeletionRuleTestsBase<PurchaseContentProductDeletionRule>
{
	public PurchaseContentProductDeletionRuleTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<PurchaseTestContext>();

	[Fact]
	public async Task ProductInPurchase_IsBlocked()
		=> await ShouldBlock(
			GetContext<PurchaseTestContext>().Product.Id,
			PurchaseContentProductDeletionBlockedMessage.Key);

	[Fact]
	public async Task ProductNotInPurchase_IsNotBlocked()
		=> await ShouldNotBlock((await CreateUnreferencedProduct()).Id);
}
