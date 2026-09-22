using System.Globalization;
using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Notification.Core.Entities;

public sealed class Notification : Entity<Notification, int>, ILinqEntity<Notification, int>
{
	public int Id { get; private set; }

	public Guid UserId { get; private set; }
	public string NotificationSystemName { get; private set; } = null!;
	public string Model { get; private set; } = null!;
	public CultureInfo? Culture { get; private set; }
	public DateTime CreateAt { get; private set; }

	private readonly List<NotificationDelivery> _deliveries = [];
	public IReadOnlyList<NotificationDelivery> Deliveries => _deliveries;

	private Notification() {}

	private Notification(
		Guid userId,
		string notificationSystemName,
		string model,
		CultureInfo? culture)
	{
		UserId = userId;
		NotificationSystemName = notificationSystemName;
		Model = model;
		Culture = culture;
		CreateAt = DateTime.UtcNow;
	}

	public static Notification Create(
		Guid userId,
		string notificationSystemName,
		string model,
		CultureInfo? culture)
		=> new(userId, notificationSystemName, model, culture);

	public NotificationDelivery MakeDelivery(string channelSystemName)
	{
		var delivery = NotificationDelivery.Create(channelSystemName);

		if (_deliveries.Any(x => string.Equals(
				x.ChannelSystemName,
				delivery.ChannelSystemName,
				StringComparison.OrdinalIgnoreCase)))
			throw new InvalidOperationException("Channel system already registered.");

		_deliveries.Add(delivery);
		return delivery;
	}

	public override int GetId() => Id;
	public static Expression<Func<Notification, int>> GetKeySelector() => x => x.Id;
	public static Expression<Func<Notification, bool>> GetEqualityExpression(int key) => x => x.Id == key;
}
