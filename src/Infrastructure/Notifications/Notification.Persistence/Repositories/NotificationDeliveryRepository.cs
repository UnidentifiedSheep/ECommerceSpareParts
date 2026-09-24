using Microsoft.EntityFrameworkCore;
using Notification.Core.Entities;
using Notification.Core.Enums;
using Notification.Core.Interfaces.Repositories;
using Notification.Persistence.Interfaces;
using Persistence.Interfaces;

namespace Notification.Persistence.Repositories;

public class NotificationDeliveryRepository(
	INotificationDbContext context,
	IQueryableExtensions queryableExtensions) : INotificationDeliveryRepository
{
	public async Task<IReadOnlyList<NotificationDelivery>> GetPendingBatchForUpdateAsync(
		string channelSystemName,
		int batchSize,
		CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(channelSystemName);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

		var query = context.NotificationDeliveries
			.Include(delivery => delivery.Notification)
			.Where(delivery => delivery.ChannelSystemName == channelSystemName &&
			                   delivery.Status == DeliveryStatus.Pending)
			.OrderBy(delivery => delivery.NotificationId)
			.Take(batchSize);

		return await queryableExtensions
			.ForUpdate(query, skipLocked: true)
			.ToListAsync(cancellationToken);
	}

	public async Task<bool> HasNextAsync(
		string channelSystemName,
		CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(channelSystemName);

		var query = context.NotificationDeliveries
			.Where(delivery => delivery.ChannelSystemName == channelSystemName &&
			                   delivery.Status == DeliveryStatus.Pending)
			.Take(1);

		var next = await queryableExtensions
			.ForUpdate(query, skipLocked: true)
			.Select(delivery => delivery.NotificationId)
			.ToListAsync(cancellationToken);

		return next.Count != 0;
	}
}
