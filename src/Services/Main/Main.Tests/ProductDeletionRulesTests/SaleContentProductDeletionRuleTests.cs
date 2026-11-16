using Main.Entities.Sale;
using Main.Modules.Sale.Application.Services.ProductDeletion;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Sale;

namespace Tests.ProductDeletionRulesTests;

public class SaleContentProductDeletionRuleTests : ProductDeletionRuleTestsBase<SaleContentProductDeletionRule>
{
	public SaleContentProductDeletionRuleTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<SaleTestContext>();

	[Fact]
	public async Task ProductInSale_IsBlocked()
		=> await ShouldBlock(GetContext<SaleTestContext>().Product.Id, SaleContentProductDeletionBlockedMessage.Key);

	[Fact]
	public async Task ProductNotInSale_IsNotBlocked()
		=> await ShouldNotBlock((await CreateUnreferencedProduct()).Id);
}
