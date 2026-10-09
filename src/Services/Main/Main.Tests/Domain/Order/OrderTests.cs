using FluentAssertions;
using Main.Entities.Order;
using Main.Enums.Orders;
using DomainOrder = Main.Entities.Order.Order;

namespace Tests.Domain.Order;

public class OrderTests
{
	[Fact]
	public void ConfirmAndComplete_FollowOrderLifecycle()
	{
		var order = Create();
		var confirmingUserId = Guid.NewGuid();

		order.Confirm(confirmingUserId);
		order.Status.Should().Be(OrderStatus.Confirmed);
		order.ConfirmedByUserId.Should().Be(confirmingUserId);
		order.ConfirmedAt.Should().NotBeNull();

		order.Complete();
		order.Status.Should().Be(OrderStatus.Completed);
	}

	[Fact]
	public void CompletedOrder_CannotBeConfirmedOrChanged()
	{
		var order = Create();
		order.Confirm(null);
		order.Complete();

		var confirm = () => order.Confirm(Guid.NewGuid());
		var cancel = order.Cancel;
		var addItem = () => order.AddItem(OrderItem.Create(order, 1, 1));
		var updateFulfillment = () => order.SetFulfillmentStatus(OrderFulfillmentStatus.FullyAvailable);

		confirm.Should().Throw<InvalidOperationException>();
		cancel.Should().Throw<InvalidOperationException>();
		addItem.Should().Throw<InvalidOperationException>();
		updateFulfillment.Should().Throw<InvalidOperationException>();
	}

	[Fact]
	public void PendingOrder_CanBeCancelledButNotCompleted()
	{
		var order = Create();

		var complete = order.Complete;
		complete.Should().Throw<InvalidOperationException>();

		order.Cancel();
		order.Status.Should().Be(OrderStatus.Cancelled);
		var confirm = () => order.Confirm(null);
		confirm.Should().Throw<InvalidOperationException>();
	}

	private static DomainOrder Create()
		=> DomainOrder.CreateOnline(
			Guid.NewGuid(),
		Guid.NewGuid(),
		1);
}
