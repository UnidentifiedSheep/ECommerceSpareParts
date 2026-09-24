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
			.Where(delivery => delivery.ChannelSystemName == channelSystemName &&
			                   delivery.Status == DeliveryStatus.Pending)
			.OrderBy(delivery => delivery.NotificationId)
			.Take(batchSize);

		return await queryableExtensions
			.ForUpdate(query, skipLocked: true)
			.ToListAsync(cancellationToken);
	}
}
