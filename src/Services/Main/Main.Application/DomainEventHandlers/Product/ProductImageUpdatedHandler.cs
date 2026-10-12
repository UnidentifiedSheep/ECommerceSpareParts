using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Products;
using Main.Application.Interfaces.Cache;
using Main.Entities.DomainEvents.Product;

namespace Main.Application.DomainEventHandlers.Product;

public class ProductImageUpdatedHandler(
	IIntegrationEventScope integrationEventScope,
	IProductCacheInvalidator productCacheInvalidator)
	: BatchableDomainEventHandler<ProductImageUpdatedDomainEvent>
{
	public override async Task Handle(
		Batch<ProductImageUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var ids = notification.Items.Select(x => x.ProductId).Distinct().ToList();
		if (ids.Count > 0)
			integrationEventScope.Add(new ProductsUpdatedEvent { Ids = ids });

		await productCacheInvalidator.InvalidateProductsAsync(ids);
	}
}
