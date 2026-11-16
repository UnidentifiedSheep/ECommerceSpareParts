using Notification.Core.Interfaces.Notification;

namespace Notification.Core;

public sealed class NotificationDefinitionBase<TNotification, TModel>(
	string systemName,
	INotificationSerializer serializer,
	Func<TModel, TNotification> createNotification)
	: INotificationDefinition<TNotification>
	where TNotification : INotification<TModel>
	where TModel : INotificationModel
{

	private readonly Func<TModel, TNotification> _createNotification =
		createNotification ?? throw new ArgumentNullException(nameof(createNotification));

	private readonly INotificationSerializer _serializer =
		serializer ?? throw new ArgumentNullException(nameof(serializer));

	public string SystemName { get; } =
		!string.IsNullOrWhiteSpace(systemName)
			? systemName
			: throw new ArgumentException("Notification system name must not be empty.", nameof(systemName));

	public TNotification FromEntity(Main.Entities.Notification.Notification entity)
	{
		ArgumentNullException.ThrowIfNull(entity);
		EnsureCanHandle(entity.NotificationSystemName);

		var model = _serializer.Deserialize<TModel>(entity.Model);
		return _createNotification(model);
	}

	public Main.Entities.Notification.Notification ToEntity(Guid userId, TNotification notification)
	{
		ArgumentNullException.ThrowIfNull(notification);
		EnsureCanHandle(notification.SystemName);

		return Main.Entities.Notification.Notification.Create(
			userId,
			SystemName,
			_serializer.Serialize(notification));
	}

	INotification INotificationDefinition.FromEntity(Main.Entities.Notification.Notification entity) => FromEntity(entity);

	Main.Entities.Notification.Notification INotificationDefinition.ToEntity(Guid userId, INotification notification)
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
		if (!string.Equals(
				SystemName,
				notificationSystemName,
				StringComparison.OrdinalIgnoreCase))
			throw new InvalidOperationException(
				$"Notification definition '{SystemName}' cannot handle " +
				$"notification '{notificationSystemName}'.");
	}
}
