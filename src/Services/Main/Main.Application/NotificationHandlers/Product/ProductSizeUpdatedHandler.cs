using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Contracts.Products;
using Main.Application.Handlers.ProductSizes.GetProductSizes;
using Main.Application.Notifications;
using MediatR;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application.NotificationHandlers;

public class ProductSizeUpdatedHandler(
    IFusionCache cache,
    ICachePolicy<GetProductSizeQuery> cachePolicy,
    IIntegrationEventScope integrationEventScope) : INotificationHandler<ProductSizeUpdatedNotification>
{
    public async Task Handle(ProductSizeUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var keys = new List<string>();
        foreach (var id in notification.ProductIds)
        {
            integrationEventScope.Add(new ProductUpdatedEvent { Id = id });
            keys.Add(cachePolicy.GetCacheKey(new GetProductSizeQuery(id)));
        }

        foreach (var key in keys) 
            await cache.RemoveAsync(key, token: cancellationToken);
    }
}