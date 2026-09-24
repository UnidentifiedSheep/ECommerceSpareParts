using System.Text.Json;
using Abstractions.Interfaces.Persistence;
using Moq;
using NamedObject.Core.Interfaces;
using Notification.Core;
using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Interfaces.Recipient;
using Notification.Core.Recipients;

namespace Notification.Tests;

public class NotificationServiceTests
{
	[Fact]
	public void RecipientJson_RoundTripsInAppRecipient()
	{
		INotificationRecipient recipient = new InAppRecipient(Guid.NewGuid());

		var json = JsonSerializer.Serialize(recipient);

		Assert.Equal(recipient, JsonSerializer.Deserialize<INotificationRecipient>(json));
	}

	[Fact]
	public async Task QueueAsync_StoresExplicitRecipientOnDelivery()
	{
		var recipient = new EmailRecipient("unconfirmed@example.com");
		var notification = new TestNotification(new TestNotificationData
		{
			TestInt = 1,
			TestString = "Verify email"
		});
		var channel = new Mock<INotificationChannel>();
		channel.SetupGet(x => x.SystemName).Returns(EmailRecipient.ChannelName);
		channel.Setup(x => x.CanHandle(notification, recipient)).Returns(true);
		var channels = new Mock<INamedObjectRegistry<INotificationChannel>>();
		channels.Setup(x => x.GetBySystemName(EmailRecipient.ChannelName)).Returns(channel.Object);
		var definition = new NotificationDefinitionBase<TestNotification, TestNotificationData>(
			notification.SystemName, new NotificationSerializer(), model => new TestNotification(model));
		var definitions = new Mock<INamedObjectRegistry<INotificationDefinition>>();
		definitions.Setup(x => x.GetBySystemName(notification.SystemName)).Returns(definition);
		Notification.Core.Entities.Notification? stored = null;
		var unitOfWork = new Mock<IUnitOfWork>();
		unitOfWork.Setup(x => x.AddRangeAsync(
			It.IsAny<IEnumerable<Notification.Core.Entities.Notification>>(),
			It.IsAny<CancellationToken>()))
			.Callback<IEnumerable<Notification.Core.Entities.Notification>, CancellationToken>((items, _) =>
				stored = Assert.Single(items))
			.Returns(Task.CompletedTask);
		var resolver = new Mock<IRecipientResolver>(MockBehavior.Strict);
		var service = new NotificationService(
			resolver.Object, definitions.Object, unitOfWork.Object, channels.Object);

		await service.QueueAsync(Guid.NewGuid(), notification, [recipient], TestContext.Current.CancellationToken);

		var delivery = Assert.Single(Assert.IsType<Notification.Core.Entities.Notification>(stored).Deliveries);
		Assert.Equal(recipient, JsonSerializer.Deserialize<INotificationRecipient>(delivery.RecipientJson!));
		resolver.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task SendAsync_SendsToExplicitRecipientWithoutQueueing()
	{
		var recipient = new EmailRecipient("unconfirmed@example.com");
		var notification = new TestNotification(new TestNotificationData
		{
			TestInt = 1,
			TestString = "Verify email"
		});
		var channel = new Mock<INotificationChannel>();
		channel.SetupGet(x => x.SystemName).Returns(EmailRecipient.ChannelName);
		channel.Setup(x => x.CanHandle(notification, recipient)).Returns(true);
		var expected = SendResult.Success();
		var cancellationToken = TestContext.Current.CancellationToken;
		channel.Setup(x => x.SendAsync(
			It.Is<NotificationDelivery>(delivery =>
				ReferenceEquals(delivery.Notification, notification) &&
				ReferenceEquals(delivery.Recipient, recipient)),
			cancellationToken)).ReturnsAsync(expected);
		var channels = new Mock<INamedObjectRegistry<INotificationChannel>>();
		channels.Setup(x => x.TryGetBySystemName(EmailRecipient.ChannelName))
			.Returns(channel.Object);
		var resolver = new Mock<IRecipientResolver>(MockBehavior.Strict);
		var definitions = new Mock<INamedObjectRegistry<INotificationDefinition>>(MockBehavior.Strict);
		var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
		var service = new NotificationService(
			resolver.Object, definitions.Object, unitOfWork.Object, channels.Object);

		var result = await service.SendAsync(recipient, notification, cancellationToken);

		Assert.Same(expected, result);
		channel.Verify(x => x.SendAsync(It.IsAny<NotificationDelivery>(), cancellationToken), Times.Once);
		resolver.VerifyNoOtherCalls();
		definitions.VerifyNoOtherCalls();
		unitOfWork.VerifyNoOtherCalls();
	}
}
