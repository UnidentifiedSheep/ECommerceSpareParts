using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Entities.Product.DomainEvents.Product;
using Main.Entities.Product.Enums;
using Main.Modules.Product.Application.Products.Interfaces;

namespace Main.Modules.Product.Application.DomainEventHandlers.Product.ProductDeleted;

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
			await eventPublisher.PublishAsync(
				ids,
				ProductEventParts.Core,
				cancellationToken);
	}
}
