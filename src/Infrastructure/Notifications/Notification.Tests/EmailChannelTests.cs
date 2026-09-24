using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Notification.Channels.Email;
using Notification.Core;
using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Recipients;
using Notification.Interfaces;
using Notification.Renderers;

namespace Notification.Tests;

public class EmailChannelTests
{
	[Fact]
	public async Task SendBatchAsync_PreservesDeliveryOrderAndNotifiesOnlySuccessfulEmails()
	{
		var notifications = Enumerable.Range(0, 4)
			.Select(_ => new Mock<INotification>().Object)
			.ToArray();
		var recipients = Enumerable.Range(0, 4)
			.Select(i => new EmailRecipient($"user{i}@example.com"))
			.ToArray();
		var deliveries = notifications
			.Zip(recipients, (notification, recipient) =>
				new NotificationDelivery<INotification, EmailRecipient>(notification, recipient))
			.ToArray();

		INotification[]? renderedNotifications = null;
		var renderer = new Mock<INotificationRenderer<INotification>>();
		renderer.Setup(x => x.TryRenderAsync(
				It.IsAny<IEnumerable<INotification>>(),
				It.IsAny<CancellationToken>()))
			.Returns<IEnumerable<INotification>, CancellationToken>((items, _) =>
			{
				renderedNotifications = items.ToArray();
				return Task.FromResult<IReadOnlyList<INotificationContent?>>([
					new NotificationContent("Body 0", "Subject 0"),
					null,
					new NotificationContent("Body 2", "Subject 2"),
					null
				]);
			});

		EmailMessage[]? sentMessages = null;
		var sender = new Mock<IEmailSender>();
		sender.Setup(x => x.SendBatchAsync(
				It.IsAny<IEnumerable<EmailMessage>>(),
				It.IsAny<CancellationToken>()))
			.Returns<IEnumerable<EmailMessage>, CancellationToken>((messages, _) =>
			{
				sentMessages = messages.ToArray();
				return Task.FromResult<IReadOnlyList<SendResult>>([
					SendResult.Success(),
					SendResult.Failure("SMTP rejected message.")
				]);
			});

		IReadOnlyCollection<EmailReceipt>? observed = null;
		var observer = new Mock<IChannelDeliveryObserver<EmailReceipt, EmailRecipient>>();
		observer.Setup(x => x.ObserveAsync(
				It.IsAny<IReadOnlyCollection<EmailReceipt>>(),
				It.IsAny<CancellationToken>()))
			.Callback<IReadOnlyCollection<EmailReceipt>, CancellationToken>((receipts, _) =>
				observed = receipts)
			.Returns(Task.CompletedTask);

		var channel = new EmailChannel(
			renderer.Object,
			sender.Object,
			[observer.Object],
			NullLogger<EmailChannel>.Instance);

		var results = await channel.SendBatchAsync(deliveries, TestContext.Current.CancellationToken);

		Assert.Equal([true, false, false, false], results.Select(x => x.Succeeded));
		Assert.Equal("Unable to render notification.", results[1].Error);
		Assert.Equal("SMTP rejected message.", results[2].Error);
		Assert.Equal("Unable to render notification.", results[3].Error);
		Assert.Equal(notifications, Assert.IsType<INotification[]>(renderedNotifications));
		renderer.Verify(x => x.TryRenderAsync(
			It.IsAny<IEnumerable<INotification>>(),
			It.IsAny<CancellationToken>()), Times.Once);
		var actualMessages = Assert.IsType<EmailMessage[]>(sentMessages);
		Assert.Equal([
			new EmailMessage("Subject 0", "user0@example.com", "Body 0"),
			new EmailMessage("Subject 2", "user2@example.com", "Body 2")
		], actualMessages);
		Assert.Equal([recipients[0]], observed?.Select(x => x.Recipient));
	}

	[Fact]
	public async Task SendBatchAsync_WhenEmpty_DoesNotRenderSendOrNotify()
	{
		var renderer = new Mock<INotificationRenderer<INotification>>();
		var sender = new Mock<IEmailSender>();
		var observer = new Mock<IChannelDeliveryObserver<EmailReceipt, EmailRecipient>>();
		var channel = CreateChannel(renderer, sender, observer);

		var results = await channel.SendBatchAsync([], TestContext.Current.CancellationToken);

		Assert.Empty(results);
		renderer.Verify(x => x.TryRenderAsync(
			It.IsAny<IEnumerable<INotification>>(),
			It.IsAny<CancellationToken>()), Times.Never);
		VerifyNotSentOrObserved(sender, observer);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(2)]
	public async Task SendBatchAsync_WhenRendererReturnsWrongCount_DoesNotSend(int renderedCount)
	{
		var renderer = new Mock<INotificationRenderer<INotification>>();
		renderer.Setup(x => x.TryRenderAsync(
				It.IsAny<IEnumerable<INotification>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(Enumerable.Repeat<INotificationContent?>(
				new NotificationContent("Body"), renderedCount).ToArray());
		var sender = new Mock<IEmailSender>();
		var observer = new Mock<IChannelDeliveryObserver<EmailReceipt, EmailRecipient>>();
		var channel = CreateChannel(renderer, sender, observer);

		await Assert.ThrowsAsync<InvalidOperationException>(() => channel.SendBatchAsync(
			[Delivery(0)], TestContext.Current.CancellationToken));

		VerifyNotSentOrObserved(sender, observer);
	}

	[Fact]
	public async Task SendBatchAsync_WhenAllRenderingIsEmpty_DoesNotSendOrNotify()
	{
		var renderer = new Mock<INotificationRenderer<INotification>>();
		renderer.Setup(x => x.TryRenderAsync(
				It.IsAny<IEnumerable<INotification>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync([null, new NotificationContent("  ")]);
		var sender = new Mock<IEmailSender>();
		var observer = new Mock<IChannelDeliveryObserver<EmailReceipt, EmailRecipient>>();
		var channel = CreateChannel(renderer, sender, observer);

		var results = await channel.SendBatchAsync(
			[Delivery(0), Delivery(1)], TestContext.Current.CancellationToken);

		Assert.Equal([false, false], results.Select(x => x.Succeeded));
		Assert.All(results, result => Assert.Equal("Unable to render notification.", result.Error));
		VerifyNotSentOrObserved(sender, observer);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(2)]
	public async Task SendBatchAsync_WhenSenderReturnsWrongCount_DoesNotNotify(int sentCount)
	{
		var renderer = new Mock<INotificationRenderer<INotification>>();
		renderer.Setup(x => x.TryRenderAsync(
				It.IsAny<IEnumerable<INotification>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync([new NotificationContent("Body")]);
		var sender = new Mock<IEmailSender>();
		sender.Setup(x => x.SendBatchAsync(
				It.IsAny<IEnumerable<EmailMessage>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(Enumerable.Repeat(SendResult.Success(), sentCount).ToArray());
		var observer = new Mock<IChannelDeliveryObserver<EmailReceipt, EmailRecipient>>();
		var channel = CreateChannel(renderer, sender, observer);

		await Assert.ThrowsAsync<InvalidOperationException>(() => channel.SendBatchAsync(
			[Delivery(0)], TestContext.Current.CancellationToken));

		sender.Verify(x => x.SendBatchAsync(
			It.IsAny<IEnumerable<EmailMessage>>(),
			It.IsAny<CancellationToken>()), Times.Once);
		observer.Verify(x => x.ObserveAsync(
			It.IsAny<IReadOnlyCollection<EmailReceipt>>(),
			It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task SendBatchAsync_WhenBatchRendererThrows_DoesNotSendOrNotify()
	{
		var failure = new InvalidOperationException("Template failed.");
		var renderer = new Mock<INotificationRenderer<INotification>>();
		renderer.Setup(x => x.TryRenderAsync(
				It.IsAny<IEnumerable<INotification>>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(failure);
		var sender = new Mock<IEmailSender>();
		var observer = new Mock<IChannelDeliveryObserver<EmailReceipt, EmailRecipient>>();
		var channel = CreateChannel(renderer, sender, observer);

		var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => channel.SendBatchAsync(
			[Delivery(0)], TestContext.Current.CancellationToken));

		Assert.Same(failure, thrown);
		VerifyNotSentOrObserved(sender, observer);
	}

	private static EmailChannel CreateChannel(
		Mock<INotificationRenderer<INotification>> renderer,
		Mock<IEmailSender> sender,
		Mock<IChannelDeliveryObserver<EmailReceipt, EmailRecipient>> observer) =>
		new(renderer.Object, sender.Object, [observer.Object], NullLogger<EmailChannel>.Instance);

	private static NotificationDelivery<INotification, EmailRecipient> Delivery(int index) =>
		new(new Mock<INotification>().Object,
			new EmailRecipient($"user{index}@example.com"));

	private static void VerifyNotSentOrObserved(
		Mock<IEmailSender> sender,
		Mock<IChannelDeliveryObserver<EmailReceipt, EmailRecipient>> observer)
	{
		sender.Verify(x => x.SendBatchAsync(
			It.IsAny<IEnumerable<EmailMessage>>(),
			It.IsAny<CancellationToken>()), Times.Never);
		observer.Verify(x => x.ObserveAsync(
			It.IsAny<IReadOnlyCollection<EmailReceipt>>(),
			It.IsAny<CancellationToken>()), Times.Never);
	}
}
