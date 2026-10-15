using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Application.Interfaces.Cache;
using Main.Entities.DomainEvents.Product;

namespace Main.Application.DomainEventHandlers.Product;

public class ProductImageUpdatedHandler(IProductCacheInvalidator productCacheInvalidator)
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
