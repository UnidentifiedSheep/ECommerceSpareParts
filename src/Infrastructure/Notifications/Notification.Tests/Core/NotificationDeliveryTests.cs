using Notification.Core.Enums;
using NotificationEntity = Notification.Core.Entities.Notification;

namespace Notification.Tests.Core;

public class NotificationDeliveryTests
{
	[Fact]
	public void Fail_FromPending_RecordsFailure()
	{
		var delivery = CreateDelivery();

		delivery.Fail("temporary error");

		Assert.Equal(DeliveryStatus.Failed, delivery.Status);
		Assert.Equal(1, delivery.Attempts);
		Assert.Equal("temporary error", delivery.Error);
		Assert.Null(delivery.DeliveredAt);
		Assert.True(delivery.IsTerminal);
	}

	[Theory]
	[InlineData("")]
	[InlineData("  ")]
	public void Fail_WithBlankError_DoesNotMutateDelivery(string error)
	{
		var delivery = CreateDelivery();

		Assert.Throws<InvalidOperationException>(() => delivery.Fail(error));
		Assert.Equal(DeliveryStatus.Pending, delivery.Status);
		Assert.Equal(0, delivery.Attempts);
	}

	[Fact]
	public void Retry_FromFailed_ReturnsDeliveryToPendingAndPreservesAttempts()
	{
		var delivery = CreateDelivery();
		delivery.Fail("temporary error");

		delivery.Retry();

		Assert.Equal(DeliveryStatus.Pending, delivery.Status);
		Assert.Equal(1, delivery.Attempts);
		Assert.Null(delivery.Error);
		Assert.False(delivery.IsTerminal);
	}

	[Fact]
	public void Retry_FromPending_DoesNothing()
	{
		var delivery = CreateDelivery();

		delivery.Retry();

		Assert.Equal(DeliveryStatus.Pending, delivery.Status);
		Assert.Equal(0, delivery.Attempts);
	}

	[Fact]
	public void MarkDelivered_FromPending_RecordsDelivery()
	{
		var delivery = CreateDelivery();
		var before = DateTime.UtcNow;

		delivery.MarkDelivered();

		Assert.Equal(DeliveryStatus.Delivered, delivery.Status);
		Assert.Equal(1, delivery.Attempts);
		Assert.Null(delivery.Error);
		Assert.InRange(delivery.DeliveredAt!.Value, before, DateTime.UtcNow);
		Assert.True(delivery.IsTerminal);
	}

	[Fact]
	public void TerminalDelivery_CannotBeCompletedAgain()
	{
		var failed = CreateDelivery();
		failed.Fail("error");
		var delivered = CreateDelivery();
		delivered.MarkDelivered();

		Assert.Throws<InvalidOperationException>(() => failed.Fail("again"));
		Assert.Throws<InvalidOperationException>(failed.MarkDelivered);
		Assert.Throws<InvalidOperationException>(() => delivered.Fail("error"));
		Assert.Throws<InvalidOperationException>(delivered.MarkDelivered);
	}

	private static Notification.Core.Entities.NotificationDelivery CreateDelivery() =>
		NotificationEntity
			.Create(Guid.NewGuid(), "test", "{}", null)
			.MakeDelivery("InApp");
}
