using Exceptions;
using FluentAssertions;
using FluentValidation.TestHelper;
using Main.Application.Orders.Commands.ChangeOrderStatus;
using Main.Entities.Exceptions;
using Main.Enums.Orders;
using Microsoft.EntityFrameworkCore;
using Tests.Abstractions.Test;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Order;

namespace Tests.HandlersTests.Orders;

public class ChangeOrderStatusTests : IntegrationTest
{
	public ChangeOrderStatusTests(CombinedContainerFixture fixture) : base(fixture) => RegisterBasicContext<OrderTestContext>();
	private OrderTestContext Orders => GetContext<OrderTestContext>();

	[Theory]
	[InlineData(OrderStatus.Confirmed)]
	[InlineData(OrderStatus.Completed)]
	[InlineData(OrderStatus.Cancelled)]
	public async Task AllowedTransition_PersistsStatusAndUsesProvidedActor(OrderStatus status)
	{
		var actorId = Orders.OtherUser.Id;
		var order = status == OrderStatus.Completed ? Orders.ConfirmedOrder : Orders.ManualOrder;
		await Mediator.Send(new ChangeOrderStatusCommand(order.Id, actorId, status, order.RowVersion), CancellationToken);
		var saved = await Context.Orders.AsNoTracking().SingleAsync(x => x.Id == order.Id, CancellationToken);
		saved.Status.Should().Be(status);
		if (status == OrderStatus.Confirmed)
		{
			saved.ConfirmedByUserId.Should().Be(actorId);
			saved.ConfirmedAt.Should().NotBeNull();
		}
	}

	[Fact]
	public async Task InvalidVersion_DoesNotChangeStatus()
	{
		var order = Orders.ManualOrder;
		await Assert.ThrowsAsync<InvalidRowVersionException>(() => Mediator.Send(
			new ChangeOrderStatusCommand(order.Id, Orders.OtherUser.Id, OrderStatus.Confirmed, order.RowVersion + 1), CancellationToken));
		(await Context.Orders.AsNoTracking().SingleAsync(x => x.Id == order.Id, CancellationToken)).Status.Should().Be(OrderStatus.Pending);
	}

	[Fact]
	public async Task CompletingPendingOrder_IsRejected()
	{
		var order = Orders.ManualOrder;
		await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(
			new ChangeOrderStatusCommand(order.Id, Orders.OtherUser.Id, OrderStatus.Completed, order.RowVersion), CancellationToken));
	}

	[Fact]
	public async Task MissingOrder_IsRejected()
	{
		await Assert.ThrowsAsync<OrderNotFoundException>(() => Mediator.Send(
			new ChangeOrderStatusCommand(Guid.NewGuid(), Orders.OtherUser.Id, OrderStatus.Confirmed, 1), CancellationToken));
	}

}

public class ChangeOrderStatusValidationTests : ValidationTest<ChangeOrderStatusValidation, ChangeOrderStatusCommand>
{
	[Theory]
	[InlineData(OrderStatus.Pending)]
	[InlineData((OrderStatus)999)]
	public async Task UnsupportedStatus_IsRejected(OrderStatus status)
	{
		var result = await ValidateAsync(new ChangeOrderStatusCommand(Guid.NewGuid(), Guid.NewGuid(), status, 1));
		result.ShouldHaveValidationErrorFor(x => x.DesiredStatus).Only();
	}

	[Fact]
	public async Task EmptyOrderId_IsRejected()
	{
		var result = await ValidateAsync(new ChangeOrderStatusCommand(Guid.Empty, Guid.NewGuid(), OrderStatus.Confirmed, 1));
		result.ShouldHaveValidationErrorFor(x => x.OrderId).Only();
	}
}
