using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Application.Interfaces.Cache;
using Main.Entities.DomainEvents.Product;

namespace Main.Application.DomainEventHandlers.Product.ProductUpdated;

public class InvalidateCacheHandler(IProductCacheInvalidator productCacheInvalidator)
	: BatchableDomainEventHandler<ProductUpdatedDomainEvent>
{
	public override async Task Handle(
		Batch<ProductUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var ids = notification.Items
			.Select(x => x.Id)
			.Distinct()
			.ToList();

		await productCacheInvalidator.InvalidateProductsAsync(ids);
	}
}
