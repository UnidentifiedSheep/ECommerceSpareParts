using FluentAssertions;
using Main.Application.Handlers.Orders;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Order;

namespace Tests.HandlersTests.Orders;

public sealed class GetOrderItemsByOrderIdsTests : IntegrationTest
{
	public GetOrderItemsByOrderIdsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<OrderTestContext>();
	}

	private OrderTestContext TestContext => GetContext<OrderTestContext>();

	[Fact]
	public async Task Batch_ReturnsRequestedItemsGroupedByOrderWithoutDuplicates()
	{
		var first = TestContext.OnlineOrder;
		var second = TestContext.ManualOrder;

		var result = await Mediator.Send(
			new GetOrderItemsByOrderIdsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]), CancellationToken);

		result.Items.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
		result.Items[first.Id].Select(item => item.Id).Should().Equal(first.Items.Select(item => item.Id).Order());
		result.Items[second.Id].Select(item => item.Id).Should().Equal(second.Items.Select(item => item.Id).Order());
	}

	[Fact]
	public async Task SingleOrder_ExcludesItemsFromOtherOrders()
	{
		var order = TestContext.ManualOrder;

		var result = await Mediator.Send(new GetOrderItemsByOrderIdsQuery(order.Id), CancellationToken);

		result.Items.Keys.Should().Equal(order.Id);
		result.Items[order.Id].Select(item => item.Id).Should().Equal(order.Items.Select(item => item.Id).Order());
	}

	[Fact]
	public async Task UnpricedItems_AreReturnedWithNullPriceAndTotal()
	{
		var order = TestContext.OnlineOrder;

		var result = await Mediator.Send(new GetOrderItemsByOrderIdsQuery(order.Id), CancellationToken);

		var items = result.Items[order.Id];
		var unpriced = items.Single(item => item.Id == order.Items.Single(item => item.UnitPrice == null).Id);
		unpriced.UnitPrice.Should().BeNull();
		unpriced.TotalPrice.Should().BeNull();
		var priced = items.Single(item => item.Id == order.Items.Single(item => item.UnitPrice != null).Id);
		priced.TotalPrice.Should().Be(251m);
	}

	[Fact]
	public async Task EmptyOrder_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(
			new GetOrderItemsByOrderIdsQuery(TestContext.EmptyOrder.Id), CancellationToken);

		result.Items.Should().BeEmpty();
	}

	[Fact]
	public async Task MissingOrder_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetOrderItemsByOrderIdsQuery(Guid.NewGuid()), CancellationToken);

		result.Items.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetOrderItemsByOrderIdsQuery(Array.Empty<Guid>()), CancellationToken);

		result.Items.Should().BeEmpty();
	}
}
