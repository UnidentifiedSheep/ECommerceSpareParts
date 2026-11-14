using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Application.Interfaces.Cache;
using Main.Entities.DomainEvents.Product;

namespace Main.Application.DomainEventHandlers.Product.ProductImageDeleted;

public class InvalidateCacheHandler(IProductCacheInvalidator productCacheInvalidator)
	: BatchableDomainEventHandler<ProductImageDeletedDomainEvent>
{
	public override Task Handle(
		Batch<ProductImageDeletedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var ids = notification.Items
			.Select(image => image.ProductId)
			.Distinct()
			.ToList();

		return productCacheInvalidator.InvalidateProductsAsync(ids);
	}
}
