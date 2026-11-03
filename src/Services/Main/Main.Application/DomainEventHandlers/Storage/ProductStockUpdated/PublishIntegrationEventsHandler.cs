using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Repositories;
using Application.Common.Services.Events;
using Contracts.Storage;
using LinqKit;
using Main.Entities.DomainEvents.ProductStock;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.DomainEventHandlers.Storage.ProductStockUpdated;

public class PublishIntegrationEventsHandler(
	IReadRepository<ProductStock, (string, int)> repository,
	IIntegrationEventScope integrationEventScope,
	TimeProvider timeProvider) : BatchableDomainEventHandler<ProductStockUpdatedDomainEvent>
{
	public override async Task Handle(
		Batch<ProductStockUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		if (notification.Items.Count == 0) return;

		var predicate = PredicateBuilder.New<ProductStock>(false);
		foreach (var item in notification.Items.Distinct())
			predicate = predicate.Or(stock => stock.ProductId == item.ProductId && stock.StorageCode == item.StorageCode);

		var stocks = await repository.Query
			.Where(predicate)
			.Select(stock => new { stock.StorageCode, stock.ProductId, stock.Quantity })
			.ToListAsync(cancellationToken);
		var occurredAt = timeProvider.GetUtcNow().UtcDateTime;
		foreach (var storage in stocks.GroupBy(stock => stock.StorageCode))
			integrationEventScope.Add(new ProductStocksUpdatedEvent
			{
				StorageCode = storage.Key,
				Items = storage.Select(stock => new ProductStockItem
				{
					ProductId = stock.ProductId,
					Quantity = stock.Quantity
				}).ToArray(),
				OccurredAt = occurredAt
			});
	}
}
