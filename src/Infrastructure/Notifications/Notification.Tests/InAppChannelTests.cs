using Abstractions.Interfaces.Persistence;
using Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Notification.Channels;
using Notification.Core.Entities;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Recipients;
using Notification.Interfaces;
using Notification.Renderers;
using Notification.Tests.TestSupport;

namespace Notification.Tests;

public class InAppChannelTests
{
	[Fact]
	public async Task SendBatchAsync_SavesRenderedRowsBeforeNotifyingAndPreservesResultOrder()
	{
		var calls = new List<string>();
		var rows = new List<InAppNotification>();
		var unitOfWork = CreateUnitOfWork();
		unitOfWork
			.Setup(x => x.ExecuteWithTransaction(
				It.IsAny<TransactionalAttribute>(),
				It.IsAny<Func<Task>>(),
				It.IsAny<CancellationToken>()))
			.Returns<TransactionalAttribute, Func<Task>, CancellationToken>(async (_, action, _) =>
			{
				calls.Add("begin");
				await action();
				calls.Add("commit");
			});
		unitOfWork
			.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<InAppNotification>>(), It.IsAny<CancellationToken>()))
			.Callback<IEnumerable<InAppNotification>, CancellationToken>((items, _) =>
			{
				calls.Add("add");
				rows.AddRange(items);
			})
			.Returns(Task.CompletedTask);
		unitOfWork
			.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
			.Callback(() =>
			{
				calls.Add("save");
				for (var i = 0; i < rows.Count; i++)
					typeof(InAppNotification).GetProperty(nameof(InAppNotification.Id))!
						.SetValue(rows[i], 100 + i);
			})
			.Returns(Task.CompletedTask);

		var observer = new RecordingInAppObserver(onObserve: receipts =>
		{
			calls.Add("observe");
			Assert.Equal([100, 101], receipts.Select(x => x.CreatedRowId));
		});
		var channel = CreateChannel(
			[new NotificationContent("First"), null, new NotificationContent("Third")],
			unitOfWork.Object,
			observer);
		var first = new InAppRecipient(Guid.NewGuid());
		var second = new InAppRecipient(Guid.NewGuid());
		var third = new InAppRecipient(Guid.NewGuid());

		var results = await channel.SendBatchAsync(
			[Delivery(first), Delivery(second), Delivery(third)],
			TestContext.Current.CancellationToken);

		Assert.Equal([true, false, true], results.Select(x => x.Succeeded));
		Assert.Equal("Unable to render notification.", results[1].Error);
		Assert.Equal(["begin", "add", "save", "observe", "commit"], calls);
		Assert.Equal([first.UserId, third.UserId], rows.Select(x => x.UserId));
		Assert.Equal(["First", "Third"], rows.Select(x => x.Text));
		Assert.Equal([first, third], Assert.Single(observer.Received).Select(x => x.Recipient));
	}

	[Fact]
	public async Task SendBatchAsync_ObserverFailureDoesNotChangeSuccessfulResult()
	{
		var unitOfWork = CreateUnitOfWork();
		var observer = new RecordingInAppObserver(
			onObserve: _ => throw new InvalidOperationException("Observer failed."));
		var channel = CreateChannel([new NotificationContent("Text")], unitOfWork.Object, observer);

		var result = await channel.SendAsync(
			Delivery(new InAppRecipient(Guid.NewGuid())),
			TestContext.Current.CancellationToken);

		Assert.True(result.Succeeded);
		Assert.Single(observer.Received);
		unitOfWork.Verify(x => x.SaveChangesAsync(
			It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task SendBatchAsync_WhenSaveFails_DoesNotNotifyObservers()
	{
		var unitOfWork = CreateUnitOfWork();
		var failure = new InvalidOperationException("Save failed.");
		unitOfWork
			.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(failure);
		var observer = new RecordingInAppObserver();
		var channel = CreateChannel([new NotificationContent("Text")], unitOfWork.Object, observer);

		var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => channel.SendBatchAsync(
			[Delivery(new InAppRecipient(Guid.NewGuid()))],
			TestContext.Current.CancellationToken));

		Assert.Same(failure, thrown);
		Assert.Empty(observer.Received);
	}

	[Fact]
	public async Task SendBatchAsync_WhenNothingRenders_DoesNotWriteOrNotify()
	{
		var unitOfWork = CreateUnitOfWork();
		var observer = new RecordingInAppObserver();
		var channel = CreateChannel([null], unitOfWork.Object, observer);

		var results = await channel.SendBatchAsync(
			[Delivery(new InAppRecipient(Guid.NewGuid()))],
			TestContext.Current.CancellationToken);

		Assert.False(Assert.Single(results).Succeeded);
		unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
		Assert.Empty(observer.Received);
	}

	private static InAppChannel CreateChannel(
		IReadOnlyList<NotificationContent?> contents,
		IUnitOfWork unitOfWork,
		RecordingInAppObserver observer)
	{
		var renderer = new Mock<INotificationRenderer<ITextNotification>>();
		renderer
			.Setup(x => x.TryRenderAsync(
				It.IsAny<IEnumerable<ITextNotification>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(contents);
		return new InAppChannel(renderer.Object, [observer], unitOfWork, NullLogger<InAppChannel>.Instance);
	}

	private static Mock<IUnitOfWork> CreateUnitOfWork()
	{
		var unitOfWork = new Mock<IUnitOfWork>();
		unitOfWork
			.Setup(x => x.ExecuteWithTransaction(
				It.IsAny<TransactionalAttribute>(),
				It.IsAny<Func<Task>>(),
				It.IsAny<CancellationToken>()))
			.Returns<TransactionalAttribute, Func<Task>, CancellationToken>((_, action, _) => action());
		return unitOfWork;
	}

	private static Notification.Core.NotificationDelivery<ITextNotification, InAppRecipient>
		Delivery(InAppRecipient recipient) => new(new Mock<ITextNotification>().Object, recipient);

}
