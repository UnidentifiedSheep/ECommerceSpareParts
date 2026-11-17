using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Entities.Product.DomainEvents.Product;
using Main.Modules.Product.Application.Interfaces.Cache;

namespace Main.Modules.Product.Application.DomainEventHandlers.Product.ProductImageUpdated;

public class InvalidateCacheHandler(IProductCacheInvalidator productCacheInvalidator)
	: BatchableDomainEventHandler<ProductImageUpdatedDomainEvent>
{
	public override async Task Handle(
		Batch<ProductImageUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var ids = notification.Items
			.Select(x => x.ProductId)
			.Distinct()
			.ToList();

		await productCacheInvalidator.InvalidateProductsAsync(ids);
	}
}
