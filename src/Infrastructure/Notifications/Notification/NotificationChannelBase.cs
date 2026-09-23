using Microsoft.Extensions.Logging;
using Notification.Core;
using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Interfaces.Recipient;

namespace Notification;

public abstract class NotificationChannelBase<TNotification, TDestination, TDestinationReceipt>(
	IEnumerable<IChannelDeliveryObserver<TDestinationReceipt, TDestination>> observers,
	ILogger logger)
	: INotificationChannel<TNotification, TDestination>
	where TNotification : INotification
	where TDestination : INotificationRecipient
	where TDestinationReceipt : IDeliveryReceipt<TDestination>
{
	private readonly IReadOnlyList<IChannelDeliveryObserver<TDestinationReceipt, TDestination>> _observers = observers.ToArray();
	protected ILogger Logger => logger;

	public abstract string SystemName { get; }

	protected async Task NotifyObserversAsync(
		IReadOnlyCollection<TDestinationReceipt> receipts,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(receipts);

		if (receipts.Count == 0) return;

		foreach (var observer in _observers)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				await observer.ObserveAsync(receipts, cancellationToken);
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch (Exception exception)
			{
				logger.LogError(exception,
					"Notification delivery observer {ObserverType} failed for channel {ChannelSystemName}.",
					observer.GetType().FullName,
					SystemName);
			}
		}
	}

	public bool CanHandle(
		INotification notification,
		INotificationRecipient recipient)
		=> notification is TNotification && recipient is TDestination;

	public virtual async Task<NotificationSendResult> SendAsync(
		NotificationDelivery<TNotification, TDestination> notification,
		CancellationToken cancellationToken = default)
	{
		var results = await SendBatchAsync([notification], cancellationToken);

		if (results.Count != 1)
			throw new InvalidOperationException(
				$"Channel '{SystemName}' returned {results.Count} results for one delivery.");

		return results[0];
	}

	public abstract Task<IReadOnlyList<NotificationSendResult>> SendBatchAsync(
		IReadOnlyCollection<NotificationDelivery<TNotification, TDestination>> notifications,
		CancellationToken cancellationToken = default);

	async Task<NotificationSendResult> INotificationChannel.SendAsync(
		NotificationDelivery notification,
		CancellationToken cancellationToken)
	{
		var typed = Convert(notification);
		return await SendAsync(typed, cancellationToken);
	}

	async Task<IReadOnlyList<NotificationSendResult>> INotificationChannel.SendBatchAsync(
		IReadOnlyCollection<NotificationDelivery> notifications,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(notifications);

		var typed = notifications.Select(Convert).ToArray();
		var results = await SendBatchAsync(typed, cancellationToken);

		if (results.Count != typed.Length)
			throw new InvalidOperationException(
				$"Channel '{SystemName}' returned {results.Count} results for {typed.Length} deliveries.");

		return results;
	}

	private NotificationDelivery<TNotification, TDestination> Convert(NotificationDelivery delivery)
	{
		ArgumentNullException.ThrowIfNull(delivery);

		if (delivery.Notification is not TNotification notification ||
			delivery.Recipient is not TDestination recipient)
			throw new InvalidOperationException(
				$"Channel '{SystemName}' cannot handle notification " +
				$"'{delivery.Notification.GetType().Name}' for recipient " +
				$"'{delivery.Recipient.GetType().Name}'.");

		return new NotificationDelivery<TNotification, TDestination>(notification, recipient);
	}
}
