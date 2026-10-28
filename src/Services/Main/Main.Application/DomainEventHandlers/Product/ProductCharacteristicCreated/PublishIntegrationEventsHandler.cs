using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Domain.Events;
using Main.Application.Interfaces.Products;
using Main.Entities.Product;
using Main.Enums;

namespace Main.Application.DomainEventHandlers.Product.ProductCharacteristicCreated;

public class PublishIntegrationEventsHandler(IProductEventPublisher eventPublisher)
	: BatchableDomainEventHandler<EntityCreatedDomainEvent<ProductCharacteristic>>
{
	public override Task Handle(
		Batch<EntityCreatedDomainEvent<ProductCharacteristic>> notification,
		CancellationToken cancellationToken) =>
		eventPublisher.PublishAsync(notification.Items.Select(x => x.Entity.ProductId), ProductEventParts.Characteristics, cancellationToken);
}
