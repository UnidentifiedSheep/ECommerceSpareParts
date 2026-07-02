using Application.Common.Interfaces.Events;
using Contracts.Products;
using Main.Application.Interfaces.Cache;
using Main.Application.Notifications;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class ProductLinkageUpdatedHandler(
    IProductCacheRepository productCacheRepository,
    IIntegrationEventScope integrationEventScope) : INotificationHandler<ProductLinkageUpdatedNotification>
{
    public async Task Handle(ProductLinkageUpdatedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var id in notification.ProductIds) 
            integrationEventScope.Add(new ProductUpdatedEvent { Id = id });
        
        await productCacheRepository.InvalidateCrossesAsync(notification.ProductIds);
    }
}