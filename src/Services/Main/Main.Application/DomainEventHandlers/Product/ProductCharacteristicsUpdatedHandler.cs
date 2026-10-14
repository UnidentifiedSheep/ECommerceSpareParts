using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Products;
using Domain.Events;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.DomainEventHandlers.Product;

public class ProductCharacteristicsUpdatedHandler(IIntegrationEventScope integrationEventScope)
	: BatchableDomainEventHandler<EntityCreatedDomainEvent<ProductCharacteristic>>,
		INotificationHandler<Batch<EntityUpdatedDomainEvent<ProductCharacteristic, (int, int)>>>,
		INotificationHandler<Batch<EntityDeletedDomainEvent<ProductCharacteristic, (int, int)>>>
{
	public override Task Handle(
		Batch<EntityCreatedDomainEvent<ProductCharacteristic>> notification,
		CancellationToken cancellationToken) =>
		PublishUpdatedEvent(notification.Items.Select(x => x.Entity.ProductId));

	public Task Handle(
		Batch<EntityUpdatedDomainEvent<ProductCharacteristic, (int, int)>> notification,
		CancellationToken cancellationToken) =>
		PublishUpdatedEvent(notification.Items.Select(x => x.Id.Item1));

	public Task Handle(
		Batch<EntityDeletedDomainEvent<ProductCharacteristic, (int, int)>> notification,
		CancellationToken cancellationToken) =>
		PublishUpdatedEvent(notification.Items.Select(x => x.Id.Item1));

	private Task PublishUpdatedEvent(IEnumerable<int> productIds)
	{
		var ids = productIds.Distinct().ToList();
		if (ids.Count > 0)
			integrationEventScope.Add(new ProductCharacteristicsUpdatedEvent { Ids = ids });

		return Task.CompletedTask;
	}
}
