using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Products;
using Main.Application.Interfaces.Cache;
using Main.Entities.DomainEvents.Product;

namespace Main.Application.DomainEventHandlers.Product;

public class ProductUpdatedHandler(
	IIntegrationEventScope integrationEventScope,
	IProductCacheInvalidator productCacheInvalidator) : BatchableDomainEventHandler<ProductUpdatedDomainEvent>
{
	public override async Task Handle(
		Batch<ProductUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var ids = notification.Items.Select(x => x.Id).Distinct().ToList();
		if (ids.Count > 0)
			integrationEventScope.Add(new ProductsUpdatedEvent { Ids = ids });

		await productCacheInvalidator.InvalidateProductsAsync(ids);
	}
}
