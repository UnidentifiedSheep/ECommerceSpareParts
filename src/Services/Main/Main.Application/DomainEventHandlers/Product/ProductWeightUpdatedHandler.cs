using Application.Common.Abstractions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Products;
using Main.Application.Handlers.ProductWeight.GetProductWeight;
using Main.Entities.DomainEvents.Product;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application.DomainEventHandlers.Product;

public class ProductWeightUpdatedHandler(
    IFusionCache cache,
    ICachePolicy<GetProductWeightQuery> cachePolicy,
    IIntegrationEventScope integrationEventScope) : BatchableDomainEventHandler<ProductWeightUpdatedDomainEvent>
{
    public override async Task Handle(Batch<ProductWeightUpdatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var keys = new List<string>(notification.Items.Count);
        foreach (var i in notification.Items)
        {
            integrationEventScope.Add(new ProductUpdatedEvent { Id = i.ProductId });
            keys.Add(cachePolicy.GetCacheKey(new GetProductWeightQuery(i.ProductId)));
        }

        foreach (var key in keys) 
            await cache.RemoveAsync(key, token: cancellationToken);
    }
}