using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Domain.Events;
using Main.Application.Interfaces.Products;
using Main.Entities.Product;
using Main.Enums;

namespace Main.Application.DomainEventHandlers.Product.ProductCharacteristicUpdated;

public class PublishIntegrationEventsHandler(IProductEventPublisher eventPublisher)
	: BatchableDomainEventHandler<EntityUpdatedDomainEvent<ProductCharacteristic, (int, int)>>
{
	public override Task Handle(
		Batch<EntityUpdatedDomainEvent<ProductCharacteristic, (int, int)>> notification,
		CancellationToken cancellationToken) =>
		eventPublisher.PublishAsync(notification.Items.Select(x => x.Id.Item1), ProductEventParts.Characteristics, cancellationToken);
}
