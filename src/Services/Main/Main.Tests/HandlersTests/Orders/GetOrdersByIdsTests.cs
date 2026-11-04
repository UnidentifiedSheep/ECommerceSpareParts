using FluentAssertions;
using Main.Application.Handlers.Orders;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Order;

namespace Tests.HandlersTests.Orders;

public sealed class GetOrdersByIdsTests : IntegrationTest
{
	public GetOrdersByIdsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<OrderTestContext>();
	}

	private OrderTestContext TestContext => GetContext<OrderTestContext>();

	[Fact]
	public async Task Batch_ReturnsOnlyRequestedOrdersWithoutDuplicates()
	{
		var first = TestContext.OnlineOrder.Id;
		var second = TestContext.ManualOrder.Id;

		var result = await Mediator.Send(
			new GetOrdersByIdsQuery([first, second, first, Guid.NewGuid()]), CancellationToken);

		result.Orders.Keys.Should().BeEquivalentTo([first, second]);
	}

	[Fact]
	public async Task SingleOrder_IsReturnedById()
	{
		var id = TestContext.ManualOrder.Id;

		var result = await Mediator.Send(new GetOrdersByIdsQuery(id), CancellationToken);

		result.Orders.Keys.Should().Equal(id);
	}

	[Fact]
	public async Task MissingOrder_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetOrdersByIdsQuery(Guid.NewGuid()), CancellationToken);

		result.Orders.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetOrdersByIdsQuery(Array.Empty<Guid>()), CancellationToken);

		result.Orders.Should().BeEmpty();
	}
}
