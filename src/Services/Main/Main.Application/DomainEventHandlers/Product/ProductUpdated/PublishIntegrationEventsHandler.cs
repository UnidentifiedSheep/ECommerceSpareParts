using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Application.Products.Interfaces;
using Main.Entities.DomainEvents.Product;
using Main.Enums;

namespace Main.Application.DomainEventHandlers.Product.ProductUpdated;

public class PublishIntegrationEventsHandler(IProductEventPublisher eventPublisher) : BatchableDomainEventHandler<ProductUpdatedDomainEvent>
{
	public override async Task Handle(
		Batch<ProductUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var ids = notification.Items.Select(x => x.Id).Distinct().ToList();
		if (ids.Count > 0)
			await eventPublisher.PublishAsync(ids, ProductEventParts.Core, cancellationToken);
	}
}
