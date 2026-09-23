using Microsoft.Extensions.Logging;
using Notification.Core;
using Notification.Core.Interfaces;
using Notification.Core.Recipients;
using Notification.Tests.TestSupport;
using Tests.Stubs;

namespace Notification.Tests;

public class NotificationChannelBaseObserverTests
{
	[Fact]
	public async Task NotifyObserversAsync_DeliversBatchToEveryObserver()
	{
		var first = new RecordingInAppObserver();
		var second = new RecordingInAppObserver();
		var channel = new TestChannel([first, second]);
		var receipts = new[]
		{
			new InAppReceipt(new InAppRecipient(Guid.NewGuid()), 1),
			new InAppReceipt(new InAppRecipient(Guid.NewGuid()), 2)
		};
		using var cancellation = new CancellationTokenSource();

		await channel.NotifyAsync(receipts, cancellation.Token);

		Assert.Same(receipts, Assert.Single(first.Received));
		Assert.Same(receipts, Assert.Single(second.Received));
		Assert.Equal(cancellation.Token, first.CancellationToken);
		Assert.Equal(cancellation.Token, second.CancellationToken);
	}

	[Fact]
	public async Task NotifyObserversAsync_WithNoReceipts_DoesNotCallObservers()
	{
		var observer = new RecordingInAppObserver();
		var channel = new TestChannel([observer]);

		await channel.NotifyAsync([], TestContext.Current.CancellationToken);

		Assert.Empty(observer.Received);
	}

	[Fact]
	public async Task NotifyObserversAsync_WithNoRegisteredObservers_IsNoOp()
	{
		var channel = new TestChannel([]);
		var receipts = new[] { new InAppReceipt(new InAppRecipient(Guid.NewGuid()), 1) };

		await channel.NotifyAsync(receipts, TestContext.Current.CancellationToken);
	}

	[Fact]
	public async Task NotifyObserversAsync_WhenObserverDoesNotThrowOnFailure_LogsErrorAndContinues()
	{
		var failure = new InvalidOperationException("Observer failed.");
		var first = new RecordingInAppObserver(failure);
		var second = new RecordingInAppObserver();
		var loggerFactory = new RecordingLoggerFactory();
		var channel = new TestChannel([first, second], loggerFactory.CreateLogger(nameof(TestChannel)));
		var receipts = new[] { new InAppReceipt(new InAppRecipient(Guid.NewGuid()), 1) };

		await channel.NotifyAsync(receipts, TestContext.Current.CancellationToken);

		Assert.Single(first.Received);
		Assert.Single(second.Received);
		Assert.Same(failure, Assert.Single(loggerFactory.Exceptions));
		Assert.Equal([LogLevel.Error], loggerFactory.LogLevels);
	}

	[Fact]
	public async Task NotifyObserversAsync_ThrowsOnlyRequiredFailuresAfterCallingEveryObserver()
	{
		var firstFailure = new InvalidOperationException("First failed.");
		var optionalFailure = new InvalidOperationException("Optional failed.");
		var secondFailure = new InvalidOperationException("Second required observer failed.");
		var first = new RecordingInAppObserver(firstFailure, throwOnFailure: true);
		var optional = new RecordingInAppObserver(optionalFailure);
		var second = new RecordingInAppObserver(secondFailure, throwOnFailure: true);
		var third = new RecordingInAppObserver();
		var loggerFactory = new RecordingLoggerFactory();
		var channel = new TestChannel([first, optional, second, third], loggerFactory.CreateLogger(nameof(TestChannel)));
		var receipts = new[] { new InAppReceipt(new InAppRecipient(Guid.NewGuid()), 1) };

		var exception = await Assert.ThrowsAsync<AggregateException>(
			() => channel.NotifyAsync(receipts, TestContext.Current.CancellationToken));

		Assert.Equal([firstFailure, secondFailure], exception.InnerExceptions);
		Assert.Single(optional.Received);
		Assert.Single(third.Received);
		Assert.Equal([firstFailure, optionalFailure, secondFailure], loggerFactory.Exceptions);
		Assert.Equal([LogLevel.Error, LogLevel.Error, LogLevel.Error], loggerFactory.LogLevels);
	}

	[Fact]
	public async Task NotifyObserversAsync_WhenOneObserverFails_RethrowsOriginalException()
	{
		var failure = new InvalidOperationException("Observer failed.");
		var loggerFactory = new RecordingLoggerFactory();
		var channel = new TestChannel(
			[new RecordingInAppObserver(failure, throwOnFailure: true)],
			loggerFactory.CreateLogger(nameof(TestChannel)));
		var receipts = new[] { new InAppReceipt(new InAppRecipient(Guid.NewGuid()), 1) };

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(
			() => channel.NotifyAsync(receipts, TestContext.Current.CancellationToken));

		Assert.Same(failure, exception);
	}

	[Fact]
	public async Task NotifyObserversAsync_WhenCanceled_StopsWithoutLogging()
	{
		var cancellation = new CancellationTokenSource();
		var first = new RecordingInAppObserver(onObserve: _ => cancellation.Cancel());
		var second = new RecordingInAppObserver();
		var loggerFactory = new RecordingLoggerFactory();
		var channel = new TestChannel([first, second], loggerFactory.CreateLogger(nameof(TestChannel)));
		var receipts = new[] { new InAppReceipt(new InAppRecipient(Guid.NewGuid()), 1) };

		await Assert.ThrowsAnyAsync<OperationCanceledException>(
			() => channel.NotifyAsync(receipts, cancellation.Token));

		Assert.Single(first.Received);
		Assert.Empty(second.Received);
		Assert.Empty(loggerFactory.Exceptions);
		Assert.Empty(loggerFactory.LogLevels);
	}

	private sealed class TestChannel(
		IEnumerable<IChannelDeliveryObserver<InAppReceipt, InAppRecipient>> observers,
		ILogger? logger = null)
		: NotificationChannelBase<TestNotification, InAppRecipient, InAppReceipt>(
			observers, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance)
	{
		public override string SystemName => InAppRecipient.ChannelName;

		public Task NotifyAsync(
			IReadOnlyCollection<InAppReceipt> receipts,
			CancellationToken cancellationToken = default) =>
			NotifyObserversAsync(receipts, cancellationToken);

		public override Task<IReadOnlyList<NotificationSendResult>> SendBatchAsync(
			IReadOnlyCollection<NotificationDelivery<TestNotification, InAppRecipient>> notifications,
			CancellationToken cancellationToken = default) =>
			Task.FromResult<IReadOnlyList<NotificationSendResult>>(
				notifications.Select(_ => NotificationSendResult.Success()).ToArray());
	}

}
