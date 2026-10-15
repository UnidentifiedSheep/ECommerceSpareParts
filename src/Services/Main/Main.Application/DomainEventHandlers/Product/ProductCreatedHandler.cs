using Application.Common.Abstractions;
using Main.Application.Interfaces.Products;
using Application.Common.Services.Events;
using Main.Entities.DomainEvents.Product;
using Main.Enums;

namespace Main.Application.DomainEventHandlers.Product;

public class ProductCreatedHandler(IProductEventPublisher eventPublisher)
	: BatchableDomainEventHandler<ProductCreatedDomainEvent>
{
	public override async Task Handle(
		Batch<ProductCreatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var ids = notification
			.Items
			.Select(x => x.Product.Id)
			.Distinct()
			.Select(x => x == 0
				? throw new InvalidOperationException(
					"Product must have an Id before integration " +
					"events are created. Call save changes before.")
				: x)
			.ToList();

		if (ids.Count > 0)
			await eventPublisher.PublishAsync(ids, ProductEventParts.All, cancellationToken);
	}
}
