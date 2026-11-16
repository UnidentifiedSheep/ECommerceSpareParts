using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Entities.Storage.DomainEvents.StorageContent;
using Main.Modules.Product.Abstractions;

namespace Main.Modules.Storage.Application.DomainEventHandlers.Storage.StorageContentCountUpdated;

public class UpdateProductStockHandler(IProductStockWriter stockWriter)
	: BatchableDomainEventHandler<StorageContentCountUpdatedDomainEvent>
{
	public override Task Handle(
		Batch<StorageContentCountUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var deltas = notification.Items
			.GroupBy(item => (item.StorageCode, item.ProductId))
			.Select(group => new ProductStockDelta(
				group.Key.StorageCode,
				group.Key.ProductId,
				checked((int)group.Sum(item => (long)item.Delta))))
			.Where(item => item.Delta != 0)
			.ToArray();

		return stockWriter.ApplyAsync(deltas, cancellationToken);
	}
}
