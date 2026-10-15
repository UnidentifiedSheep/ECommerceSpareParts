using Application.Common.Abstractions;
using Main.Application.Interfaces.Products;
using Application.Common.Services.Events;
using Main.Entities.DomainEvents.Product;

namespace Main.Application.DomainEventHandlers.Product;

public class ProductWeightUpdatedHandler(IProductEventPublisher eventPublisher)
	: BatchableDomainEventHandler<ProductWeightUpdatedDomainEvent>
{
	public override async Task Handle(
		Batch<ProductWeightUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var ids = notification.Items.Select(x => x.ProductId).Distinct().ToList();
		if (ids.Count > 0)
			await eventPublisher.PublishAsync(ids, ProductEventParts.Weight, cancellationToken);
	}
}
