using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Products;
using Main.Application.Interfaces.Cache;
using Main.Entities.DomainEvents.Product;

namespace Main.Application.DomainEventHandlers.Product;

public class ProductUpdatedHandler(
    IIntegrationEventScope integrationEventScope,
    IProductCacheRepository productCacheRepository
    ) : BatchableDomainEventHandler<ProductUpdatedDomainEvent>
{
    public override async Task Handle(Batch<ProductUpdatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var ids = new List<int>(notification.Items.Count);
        foreach (var @event in notification.Items)
        {
            integrationEventScope.Add(new ProductUpdatedEvent
            {
                Id = @event.Id
            });
            ids.Add(@event.Id);
        }
        
        await productCacheRepository.InvalidateProductsAsync(ids);
    }
}