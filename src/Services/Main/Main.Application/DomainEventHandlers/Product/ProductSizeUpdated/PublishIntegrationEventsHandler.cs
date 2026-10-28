using Application.Common.Abstractions;
using Main.Application.Interfaces.Products;
using Application.Common.Services.Events;
using Main.Entities.DomainEvents.Product;
using Main.Enums;

namespace Main.Application.DomainEventHandlers.Product.ProductSizeUpdated;

public class PublishIntegrationEventsHandler(IProductEventPublisher eventPublisher)
	: BatchableDomainEventHandler<ProductSizeUpdatedDomainEvent>
{
	public override async Task Handle(
		Batch<ProductSizeUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var ids = notification.Items.Select(x => x.ProductId).Distinct().ToList();
		if (ids.Count > 0)
			await eventPublisher.PublishAsync(ids, ProductEventParts.Dimensions, cancellationToken);
	}
}
