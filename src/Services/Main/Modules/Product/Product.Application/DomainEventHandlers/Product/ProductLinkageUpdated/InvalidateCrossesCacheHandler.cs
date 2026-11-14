using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Application.Interfaces.Cache;
using Main.Entities.DomainEvents.Product;

namespace Main.Application.DomainEventHandlers.Product.ProductLinkageUpdated;

public class InvalidateCrossesCacheHandler(IProductCacheInvalidator productCacheInvalidator)
	: BatchableDomainEventHandler<ProductLinkageUpdatedDomainEvent>
{
	public override Task Handle(
		Batch<ProductLinkageUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var ids = notification.Items
			.Select(x => x.ProductId)
			.Distinct()
			.ToList();
		return productCacheInvalidator.InvalidateCrossesAsync(ids);
	}
}
