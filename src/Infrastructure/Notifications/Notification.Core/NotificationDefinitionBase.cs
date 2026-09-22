using Notification.Core.Interfaces;

namespace Notification.Core;

public abstract class NotificationDefinitionBase<TNotification>(
	INotificationSerializer serializer
	) : INotificationDefinition<TNotification>
	where TNotification : INotification
{
	public abstract string SystemName { get; }

	public TNotification FromEntity(Entities.Notification entity)
	{
		ArgumentNullException.ThrowIfNull(entity);
		EnsureCanHandle(entity.NotificationSystemName);

		return FromEntityCore(entity);
	}

	protected abstract TNotification FromEntityCore(Entities.Notification entity);

	public Entities.Notification ToEntity(Guid userId, TNotification notification)
	{
		ArgumentNullException.ThrowIfNull(notification);
		EnsureCanHandle(notification.SystemName);

		return Entities.Notification.Create(
			userId,
			SystemName,
			serializer.Serialize(notification),
			notification.SelectedCulture);
	}

	INotification INotificationDefinition.FromEntity(Entities.Notification entity)
		=> FromEntity(entity);

	Entities.Notification INotificationDefinition.ToEntity(
		Guid userId,
		INotification notification)
	{
		ArgumentNullException.ThrowIfNull(notification);

		if (notification is not TNotification typedNotification)
			throw new InvalidOperationException(
				$"Notification definition '{SystemName}' cannot handle " +
				$"notification type '{notification.GetType().Name}'.");

		return ToEntity(userId, typedNotification);
	}

	private void EnsureCanHandle(string notificationSystemName)
	{
		if (!string.Equals(SystemName, notificationSystemName, StringComparison.OrdinalIgnoreCase))
			throw new InvalidOperationException(
				$"Notification definition '{SystemName}' cannot handle " +
				$"notification '{notificationSystemName}'.");
	}
}
