using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Products;
using Main.Entities.DomainEvents.Product;

namespace Main.Application.DomainEventHandlers.Product;

public class ProductSizeUpdatedHandler(IIntegrationEventScope integrationEventScope)
	: BatchableDomainEventHandler<ProductSizeUpdatedDomainEvent>
{
	public override Task Handle(
		Batch<ProductSizeUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var ids = notification.Items.Select(x => x.ProductId).Distinct().ToList();
		if (ids.Count > 0)
			integrationEventScope.Add(new ProductsUpdatedEvent { Ids = ids });

		return Task.CompletedTask;
	}
}
