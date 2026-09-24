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
