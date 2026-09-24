using NotificationDeliveryEntity = Notification.Core.Entities.NotificationDelivery;

namespace Notification.Core.Interfaces.Repositories;

public interface INotificationDeliveryRepository
{
	Task<IReadOnlyList<NotificationDeliveryEntity>> GetPendingBatchForUpdateAsync(
		string channelSystemName,
		int batchSize,
		CancellationToken cancellationToken = default);
}
