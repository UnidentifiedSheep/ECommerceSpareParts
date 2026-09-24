using Abstractions.Interfaces.Persistence;
using NamedObject.Core.Interfaces;
using Notification.Core;
using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Interfaces.Recipient;
using NotificationEntity = Notification.Core.Entities.Notification;

namespace Notification;

public class NotificationService(
	IRecipientResolver recipientResolver,
	INamedObjectRegistry<INotificationDefinition> definitionRegistry,
	IUnitOfWork unitOfWork,
	INamedObjectRegistry<INotificationChannel> channelsRegistry
	) : INotificationService
{
	public Task<SendResult> SendAsync(
		INotificationRecipient recipient,
		INotification notification,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(recipient);
		ArgumentNullException.ThrowIfNull(notification);

		var channel = channelsRegistry.TryGetBySystemName(recipient.ChannelSystemName) ??
			throw new InvalidOperationException(
				$"Notification channel '{recipient.ChannelSystemName}' is not registered.");

		if (!channel.CanHandle(notification, recipient))
			throw new InvalidOperationException(
				$"Channel '{channel.SystemName}' cannot handle notification " +
				$"'{notification.GetType().Name}' for recipient '{recipient.GetType().Name}'.");

		return channel.SendAsync(new NotificationDelivery(notification, recipient), cancellationToken);
	}

	public async Task QueueAsync(
		NotificationItem notification,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(notification);

		var recipients = await recipientResolver
			.ResolveAsync(notification.UserId, cancellationToken);
		var entity = Create(notification, recipients);

		if (entity is not null)
			await unitOfWork.AddRangeAsync([entity], cancellationToken);
	}

	public async Task QueueAsync(
		IReadOnlyCollection<NotificationItem> notifications,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(notifications);

		if (notifications.Count == 0) return;

		var userIds = notifications
			.Select(x => x.UserId)
			.Distinct()
			.ToArray();
		var recipientsByUser = await recipientResolver
			.ResolveAsync(userIds, cancellationToken);
		var entities = new List<NotificationEntity>(notifications.Count);

		foreach (var notification in notifications)
		{
			if (!recipientsByUser.TryGetValue(notification.UserId, out var recipients))
				continue;

			var entity = Create(notification, recipients);
			if (entity is not null) entities.Add(entity);
		}

		if (entities.Count != 0)
			await unitOfWork.AddRangeAsync(entities, cancellationToken);
	}

	private NotificationEntity? Create(
		NotificationItem item,
		IReadOnlyCollection<INotificationRecipient> recipients)
	{
		var channelSystemNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		foreach (var recipientsByChannel in recipients
			.Where(x => !string.IsNullOrWhiteSpace(x.ChannelSystemName))
			.GroupBy(x => x.ChannelSystemName, StringComparer.OrdinalIgnoreCase))
		{
			var channel = channelsRegistry.TryGetBySystemName(recipientsByChannel.Key) ??
				throw new InvalidOperationException(
					$"Notification channel '{recipientsByChannel.Key}' is not registered.");

			if (recipientsByChannel.Any(recipient => channel.CanHandle(item.Notification, recipient)))
				channelSystemNames.Add(channel.SystemName);
		}

		if (channelSystemNames.Count == 0) return null;

		var definition = definitionRegistry.GetBySystemName(item.Notification.SystemName);
		var notification = definition.ToEntity(item.UserId, item.Notification);

		foreach (var channelSystemName in channelSystemNames)
			notification.MakeDelivery(channelSystemName);

		return notification;
	}
}
