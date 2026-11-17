using Application.Common.Abstractions;
using Application.Common.Services.Events;
using Main.Entities.Product.DomainEvents.Product;
using Main.Modules.Product.Application.Interfaces.Cache;

namespace Main.Modules.Product.Application.DomainEventHandlers.Product.ProductLinkageUpdated;

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
