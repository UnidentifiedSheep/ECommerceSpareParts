using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Domain.Validation;
using Notification.Core.Enums;

namespace Notification.Core.Entities;

public sealed class NotificationDelivery :
	Entity<NotificationDelivery, NotificationDeliveryKey>,
	ILinqEntity<NotificationDelivery, NotificationDeliveryKey>
{
	public int NotificationId { get; private set; }
	public Notification Notification { get; private set; } = null!;
	public string ChannelSystemName { get; private set; } = null!;
	public DeliveryStatus Status { get; private set; }
	public int Attempts { get; private set; }
	public string? Error { get; private set; }
	public DateTime? DeliveredAt { get; private set; }

	public bool IsTerminal => Status is DeliveryStatus.Failed or DeliveryStatus.Delivered;

	private NotificationDelivery() { }

	private NotificationDelivery(Notification notification, string channelSystemName)
	{
		Notification = notification ?? throw new ArgumentNullException(nameof(notification));
		ChannelSystemName = channelSystemName
			.EnsureNotNullOrWhiteSpace(
				() => new InvalidOperationException("Channel system name must not be null or empty."))
			.Trim();
		Status = DeliveryStatus.Pending;
	}

	internal static NotificationDelivery Create(Notification notification, string channelSystemName)
		=> new(notification, channelSystemName);

	public void Retry()
	{
		if (Status is not DeliveryStatus.Failed) return;

		Error = null;
		Status = DeliveryStatus.Pending;
	}

	public void Fail(string error, int maxAttempts)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxAttempts);
		if (IsTerminal)
			throw new InvalidOperationException("Cannot fail notification in terminal state.");

		var validatedError = error.EnsureNotNullOrWhiteSpace(
			() => new InvalidOperationException("Delivery error must not be null or empty."));

		Attempts++;
		Error = validatedError;
		Status = DeliveryStatus.Failed;
		if (Attempts < maxAttempts)
			Retry();
	}

	public void MarkDelivered()
	{
		if (IsTerminal)
			throw new InvalidOperationException("Cannot mark notification as delivered in terminal state.");

		Attempts++;
		Error = null;
		DeliveredAt = DateTime.UtcNow;
		Status = DeliveryStatus.Delivered;
	}

	public override NotificationDeliveryKey GetId() => new(NotificationId, ChannelSystemName);
	public static Expression<Func<NotificationDelivery, NotificationDeliveryKey>> GetKeySelector()
		=> n => new NotificationDeliveryKey(n.NotificationId, n.ChannelSystemName);
	public static Expression<Func<NotificationDelivery, bool>> GetEqualityExpression(NotificationDeliveryKey key)
		=> n => n.NotificationId == key.NotificationId && n.ChannelSystemName == key.ChannelSystemName;
}

public readonly record struct NotificationDeliveryKey(int NotificationId, string ChannelSystemName) : ICompositeKey
{
	public object[] ToArray() => [NotificationId, ChannelSystemName];
}
