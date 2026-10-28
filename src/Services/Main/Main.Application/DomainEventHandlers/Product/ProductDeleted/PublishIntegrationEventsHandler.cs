using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Application.Interfaces.Products;
using Main.Entities.DomainEvents.Product;
using Main.Enums;

namespace Main.Application.DomainEventHandlers.Product.ProductDeleted;

public class PublishIntegrationEventsHandler(IProductEventPublisher eventPublisher)
	: BatchableDomainEventHandler<ProductDeletedDomainEvent>
{
	public override async Task Handle(
		Batch<ProductDeletedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var ids = notification.Items
			.Select(product => product.ProductId)
			.Distinct()
			.ToList();

		if (ids.Count > 0)
			await eventPublisher.PublishAsync(ids, ProductEventParts.Core, cancellationToken);
	}
}
