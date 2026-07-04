using Application.Common.Abstractions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Products;
using Main.Application.Handlers.ProductSizes.GetProductSizes;
using Main.Entities.DomainEvents.Product;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application.DomainEventHandlers.Product;

public class ProductSizeUpdatedHandler(
    IFusionCache cache,
    ICachePolicy<GetProductSizeQuery> cachePolicy,
    IIntegrationEventScope integrationEventScope) : BatchableDomainEventHandler<ProductSizeUpdatedDomainEvent>
{
    public override async Task Handle(Batch<ProductSizeUpdatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var keys = new List<string>(notification.Items.Count);
        foreach (var @event in notification.Items)
        {
            integrationEventScope.Add(new ProductUpdatedEvent { Id = @event.ProductId });
            keys.Add(cachePolicy.GetCacheKey(new GetProductSizeQuery(@event.ProductId)));
        }

        foreach (var key in keys)
            await cache.RemoveAsync(key, token: cancellationToken);
    }
}