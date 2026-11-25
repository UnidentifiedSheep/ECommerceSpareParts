using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Entities.Logistics.DomainEvents;
using Main.Modules.Logistics.Application.Interfaces;

namespace Main.Modules.Logistics.Application.DomainEventHandlers.DeliveryScheduleChanged;

public sealed class InvalidateCacheHandler(IDeliveryScheduleCacheRepository cacheRepository)
	: BatchableDomainEventHandler<DeliveryScheduleChangedDomainEvent>
{
	public override async Task Handle(
		Batch<DeliveryScheduleChangedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		foreach (var item in notification.Items.DistinctBy(x => (x.Supplier, x.StorageCode)))
			await cacheRepository.InvalidateAsync(item.Supplier, item.StorageCode, cancellationToken);
	}
}
