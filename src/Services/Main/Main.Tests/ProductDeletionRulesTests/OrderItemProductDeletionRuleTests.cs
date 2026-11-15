using Main.Entities;
using Main.Application.Services.ProductDeletion;
using Main.Entities.Product;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Order;

namespace Tests.ProductDeletionRulesTests;

public class OrderItemProductDeletionRuleTests : ProductDeletionRuleTestsBase<OrderItemProductDeletionRule>
{
	public OrderItemProductDeletionRuleTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<OrderTestContext>();

	[Fact]
	public async Task ProductInOrder_IsBlocked()
		=> await ShouldBlock(GetContext<ProductTestContext>().Products[0].Id, OrderItemProductDeletionBlockedMessage.Key);

	[Fact]
	public async Task ProductNotInOrder_IsNotBlocked()
		=> await ShouldNotBlock((await CreateUnreferencedProduct()).Id);
}
