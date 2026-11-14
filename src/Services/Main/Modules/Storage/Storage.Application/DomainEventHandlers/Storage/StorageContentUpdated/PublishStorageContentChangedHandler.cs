using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Main.Entities.DomainEvents.StorageContent;
using Main.Modules.Storage.Abstractions.Events;

namespace Main.Application.DomainEventHandlers.Storage.StorageContentUpdated;

public class PublishStorageContentChangedHandler(IDomainEventScope domainEventScope)
	: BatchableDomainEventHandler<StorageContentUpdatedDomainEvent>
{
	public override Task Handle(
		Batch<StorageContentUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var now = DateTime.UtcNow;
		var snapshots = new Dictionary<int, StorageContentSnapshot>();
		foreach (var item in notification.Items)
		{
			if (item.Content.Id == 0)
				throw new InvalidOperationException("Storage content must be saved before.");
			snapshots[item.Content.Id] = new StorageContentSnapshot(
				item.Content.Id,
				item.Content.ProductId,
				item.Content.StorageCode,
				item.Content.CurrencyId,
				item.Content.BuyPrice,
				item.Deleted ? 0 : item.Content.Count);
		}

		domainEventScope.AddRange(snapshots.Values.Select(x => new StorageContentChanged(x, now)));
		return Task.CompletedTask;
	}
}
