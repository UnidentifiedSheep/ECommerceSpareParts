using FluentAssertions;
using Exceptions;
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
		var addItem = () => order.AddItem(1, 1);
		var updateFulfillment = () => order.SetFulfillmentStatus(OrderFulfillmentStatus.FullyAvailable);

		confirm.Should().Throw<InvalidInputException>();
		cancel.Should().Throw<InvalidInputException>();
		addItem.Should().Throw<InvalidInputException>();
		updateFulfillment.Should().Throw<InvalidInputException>();
	}

	[Fact]
	public void PendingOrder_CanBeCancelledButNotCompleted()
	{
		var order = Create();

		var complete = order.Complete;
		complete.Should().Throw<InvalidInputException>();

		order.Cancel();
		order.Status.Should().Be(OrderStatus.Cancelled);
		var confirm = () => order.Confirm(null);
		confirm.Should().Throw<InvalidInputException>();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void PendingOrConfirmedOrder_CanChangeItems(bool confirmed)
	{
		var order = Create();
		var item = order.AddItem(1, 1);
		if (confirmed) order.Confirm(null);

		order.ChangeItemCount(item, 3);
		order.ChangeItemProduct(item, 2);
		order.ChangeItemPrice(item, 12m);

		item.Count.Should().Be(3);
		item.ProductId.Should().Be(2);
		item.UnitPrice.Should().Be(12m);
	}

	private static DomainOrder Create()
		=> DomainOrder.CreateOnline(
			Guid.NewGuid(),
		Guid.NewGuid(),
		1);
}
