using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Entities.Product.DomainEvents.Product;
using Main.Entities.Product.Enums;
using Main.Modules.Product.Application.Products.Interfaces;

namespace Main.Modules.Product.Application.DomainEventHandlers.Product.ProductWeightUpdated;

public class PublishIntegrationEventsHandler(IProductEventPublisher eventPublisher)
	: BatchableDomainEventHandler<ProductWeightUpdatedDomainEvent>
{
	public override async Task Handle(
		Batch<ProductWeightUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var ids = notification.Items.Select(x => x.ProductId).Distinct().ToList();
		if (ids.Count > 0)
			await eventPublisher.PublishAsync(
				ids,
				ProductEventParts.Weight,
				cancellationToken);
	}
}
