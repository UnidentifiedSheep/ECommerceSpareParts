using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Application.Interfaces.Cache;
using Main.Entities.DomainEvents.Product;

namespace Main.Application.DomainEventHandlers.Product.ProductDeleted;

public class InvalidateCacheHandler(
	IProductCacheInvalidator productCacheInvalidator
	) : BatchableDomainEventHandler<ProductDeletedDomainEvent>
{
	public async override Task Handle(
		Batch<ProductDeletedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var ids = notification.Items
			.Select(x => x.ProductId)
			.Distinct()
			.ToList();

		await productCacheInvalidator.InvalidateCrossesAsync(ids);
		await productCacheInvalidator.InvalidateProductsAsync(ids);
	}
}
