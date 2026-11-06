using FluentAssertions;
using Enums;
using Main.Application.Orders.Queries;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Security.Core.Interfaces;
using Tests.Stubs;
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

		var result = await Send(
			new GetOrderItemsByOrderIdsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]));

		result.Items.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
		result.Items[first.Id].Select(item => item.Id).Should().Equal(first.Items.Select(item => item.Id).Order());
		result.Items[second.Id].Select(item => item.Id).Should().Equal(second.Items.Select(item => item.Id).Order());
	}

	[Fact]
	public async Task SingleOrder_ExcludesItemsFromOtherOrders()
	{
		var order = TestContext.ManualOrder;

		var result = await Send(new GetOrderItemsByOrderIdsQuery(order.Id));

		result.Items.Keys.Should().Equal(order.Id);
		result.Items[order.Id].Select(item => item.Id).Should().Equal(order.Items.Select(item => item.Id).Order());
	}

	[Fact]
	public async Task UnpricedItems_AreReturnedWithNullPriceAndTotal()
	{
		var order = TestContext.OnlineOrder;

		var result = await Send(new GetOrderItemsByOrderIdsQuery(order.Id));

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
		var result = await Send(
			new GetOrderItemsByOrderIdsQuery(TestContext.EmptyOrder.Id));

		result.Items.Should().BeEmpty();
	}

	[Fact]
	public async Task MissingOrder_ReturnsEmptyDictionary()
	{
		var result = await Send(new GetOrderItemsByOrderIdsQuery(Guid.NewGuid()));

		result.Items.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Send(new GetOrderItemsByOrderIdsQuery(Array.Empty<Guid>()));

		result.Items.Should().BeEmpty();
	}

	[Fact]
	public async Task OwnPermission_ExcludesOtherUsersAndUnassignedOrders()
	{
		var result = await Send(new GetOrderItemsByOrderIdsQuery([
			TestContext.OnlineOrder.Id, TestContext.ManualOrder.Id, TestContext.ConfirmedOrder.Id]),
			PermissionCodes.ORDERS_GET_OWN);

		result.Items.Keys.Should().Equal(TestContext.OnlineOrder.Id);
	}

	[Fact]
	public async Task OwnPermission_OtherUsersOrderReturnsEmptyResult()
	{
		var result = await Send(new GetOrderItemsByOrderIdsQuery(TestContext.ConfirmedOrder.Id), PermissionCodes.ORDERS_GET_OWN);

		result.Items.Should().BeEmpty();
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task MissingPermissionOrAuthentication_DeniesAccess(bool authenticated)
	{
		var user = (UserContextMock)Scope.ServiceProvider.GetRequiredService<IUserContext>();
		user.SetIsAuthenticated(authenticated);

		await Assert.ThrowsAsync<OrderReadAccessDeniedException>(() => Send(
			new GetOrderItemsByOrderIdsQuery(TestContext.OnlineOrder.Id), authenticated ? null : PermissionCodes.ORDERS_GET));
	}

	private Task<GetOrderItemsByOrderIdsResult> Send(GetOrderItemsByOrderIdsQuery query, PermissionCodes? permission = PermissionCodes.ORDERS_GET)
	{
		if (permission.HasValue)
			((UserContextMock)Scope.ServiceProvider.GetRequiredService<IUserContext>())
				.SetPermissions([Permission.ToNormalizedPermission(permission.Value)]);
		return Mediator.Send(query, CancellationToken);
	}
}
