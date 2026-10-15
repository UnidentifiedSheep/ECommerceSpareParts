using Application.Common.Abstractions;
using Main.Application.Interfaces.Products;
using Application.Common.Services.Events;
using Domain.Events;
using Main.Entities.Product;
using Main.Enums;
using MediatR;

namespace Main.Application.DomainEventHandlers.Product;

public class ProductCharacteristicsUpdatedHandler(IProductEventPublisher eventPublisher)
	: BatchableDomainEventHandler<EntityCreatedDomainEvent<ProductCharacteristic>>,
		INotificationHandler<Batch<EntityUpdatedDomainEvent<ProductCharacteristic, (int, int)>>>,
		INotificationHandler<Batch<EntityDeletedDomainEvent<ProductCharacteristic, (int, int)>>>
{
	public override Task Handle(
		Batch<EntityCreatedDomainEvent<ProductCharacteristic>> notification,
		CancellationToken cancellationToken) =>
		eventPublisher.PublishAsync(notification.Items.Select(x => x.Entity.ProductId), ProductEventParts.Characteristics, cancellationToken);

	public Task Handle(
		Batch<EntityUpdatedDomainEvent<ProductCharacteristic, (int, int)>> notification,
		CancellationToken cancellationToken) =>
		eventPublisher.PublishAsync(notification.Items.Select(x => x.Id.Item1), ProductEventParts.Characteristics, cancellationToken);

	public Task Handle(
		Batch<EntityDeletedDomainEvent<ProductCharacteristic, (int, int)>> notification,
		CancellationToken cancellationToken) =>
		eventPublisher.PublishAsync(notification.Items.Select(x => x.Id.Item1), ProductEventParts.Characteristics, cancellationToken);
}
