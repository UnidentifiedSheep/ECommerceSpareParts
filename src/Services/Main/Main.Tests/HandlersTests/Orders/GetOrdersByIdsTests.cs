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

		var result = await Send(
			new GetOrdersByIdsQuery([first, second, first, Guid.NewGuid()]));

		result.Orders.Keys.Should().BeEquivalentTo([first, second]);
	}

	[Fact]
	public async Task SingleOrder_IsReturnedById()
	{
		var id = TestContext.ManualOrder.Id;

		var result = await Send(new GetOrdersByIdsQuery(id));

		result.Orders.Keys.Should().Equal(id);
	}

	[Fact]
	public async Task MissingOrder_ReturnsEmptyDictionary()
	{
		var result = await Send(new GetOrdersByIdsQuery(Guid.NewGuid()));

		result.Orders.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Send(new GetOrdersByIdsQuery(Array.Empty<Guid>()));

		result.Orders.Should().BeEmpty();
	}

	[Fact]
	public async Task OwnPermission_ExcludesOtherUsersAndUnassignedOrders()
	{
		var result = await Send(new GetOrdersByIdsQuery([
			TestContext.OnlineOrder.Id, TestContext.ManualOrder.Id, TestContext.ConfirmedOrder.Id]),
			PermissionCodes.ORDERS_GET_OWN);

		result.Orders.Keys.Should().Equal(TestContext.OnlineOrder.Id);
	}

	[Fact]
	public async Task OwnPermission_OtherUsersOrderReturnsEmptyResult()
	{
		var result = await Send(new GetOrdersByIdsQuery(TestContext.ConfirmedOrder.Id), PermissionCodes.ORDERS_GET_OWN);

		result.Orders.Should().BeEmpty();
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task MissingPermissionOrAuthentication_DeniesAccess(bool authenticated)
	{
		var user = (UserContextMock)Scope.ServiceProvider.GetRequiredService<IUserContext>();
		user.SetIsAuthenticated(authenticated);

		await Assert.ThrowsAsync<OrderReadAccessDeniedException>(() => Send(
			new GetOrdersByIdsQuery(TestContext.OnlineOrder.Id), authenticated ? null : PermissionCodes.ORDERS_GET));
	}

	private Task<GetOrdersByIdsResult> Send(GetOrdersByIdsQuery query, PermissionCodes? permission = PermissionCodes.ORDERS_GET)
	{
		if (permission.HasValue)
			((UserContextMock)Scope.ServiceProvider.GetRequiredService<IUserContext>())
				.SetPermissions([Permission.ToNormalizedPermission(permission.Value)]);
		return Mediator.Send(query, CancellationToken);
	}
}
