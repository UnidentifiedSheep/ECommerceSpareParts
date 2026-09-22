using System.Globalization;
using Notification.Core.Enums;
using NotificationEntity = Notification.Core.Entities.Notification;

namespace Notification.Tests.Core;

public class NotificationEntityTests
{
	[Fact]
	public void Create_WithValidValues_InitializesNotification()
	{
		var userId = Guid.NewGuid();
		var culture = CultureInfo.GetCultureInfo("ru-RU");
		var before = DateTime.UtcNow;

		var notification = NotificationEntity.Create(userId, "order-created", "{\"id\":42}", culture);

		Assert.Equal(userId, notification.UserId);
		Assert.Equal("order-created", notification.NotificationSystemName);
		Assert.Equal("{\"id\":42}", notification.Model);
		Assert.Same(culture, notification.Culture);
		Assert.InRange(notification.CreateAt, before, DateTime.UtcNow);
		Assert.Empty(notification.Deliveries);
	}

	[Theory]
	[InlineData("")]
	[InlineData("not-json")]
	[InlineData("{broken}")]
	public void Create_WithInvalidJson_Throws(string model)
	{
		var action = () => NotificationEntity.Create(Guid.NewGuid(), "test", model, null);

		var exception = Assert.Throws<InvalidOperationException>(action);
		Assert.Equal("Model must be valid json value.", exception.Message);
	}

	[Fact]
	public void MakeDelivery_TrimsChannelNameAndStartsPending()
	{
		var notification = CreateNotification();

		var delivery = notification.MakeDelivery("  InApp  ");

		Assert.Equal("InApp", delivery.ChannelSystemName);
		Assert.Equal(DeliveryStatus.Pending, delivery.Status);
		Assert.Equal(0, delivery.Attempts);
		Assert.False(delivery.IsTerminal);
		Assert.Same(delivery, Assert.Single(notification.Deliveries));
	}

	[Fact]
	public void MakeDelivery_WithDuplicateChannelIgnoringCase_Throws()
	{
		var notification = CreateNotification();
		notification.MakeDelivery("InApp");

		var exception = Assert.Throws<InvalidOperationException>(() => notification.MakeDelivery("inapp"));

		Assert.Equal("Channel system already registered.", exception.Message);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void MakeDelivery_WithBlankChannel_Throws(string channel)
	{
		var exception = Assert.Throws<InvalidOperationException>(() => CreateNotification().MakeDelivery(channel));

		Assert.Equal("Channel system name must not be null or empty.", exception.Message);
	}

	private static NotificationEntity CreateNotification() =>
		NotificationEntity.Create(Guid.NewGuid(), "test", "{}", null);
}
