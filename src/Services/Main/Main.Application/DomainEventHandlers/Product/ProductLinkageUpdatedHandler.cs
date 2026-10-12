using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Products;
using Main.Application.Interfaces.Cache;
using Main.Entities.DomainEvents.Product;

namespace Main.Application.DomainEventHandlers.Product;

public class ProductLinkageUpdatedHandler(
	IProductCacheInvalidator productCacheInvalidator,
	IIntegrationEventScope integrationEventScope)
	: BatchableDomainEventHandler<ProductLinkageUpdatedDomainEvent>
{
	public override async Task Handle(
		Batch<ProductLinkageUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var ids = notification.Items.Select(x => x.ProductId).Distinct().ToList();
		if (ids.Count > 0)
			integrationEventScope.Add(new ProductsUpdatedEvent { Ids = ids });

		await productCacheInvalidator.InvalidateCrossesAsync(ids);
	}
}
