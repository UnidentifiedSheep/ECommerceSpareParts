using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Entities.Product.DomainEvents.Product;
using Main.Entities.Product.Enums;
using Main.Modules.Product.Application.Products.Interfaces;

namespace Main.Modules.Product.Application.DomainEventHandlers.Product.ProductCreated;

public class PublishIntegrationEventsHandler(IProductEventPublisher eventPublisher)
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
			await eventPublisher.PublishAsync(
				ids,
				ProductEventParts.All,
				cancellationToken);
	}
}
