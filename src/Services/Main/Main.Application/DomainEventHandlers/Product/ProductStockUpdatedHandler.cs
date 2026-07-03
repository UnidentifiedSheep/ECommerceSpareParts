using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Products;
using Main.Entities.DomainEvents.Product;

namespace Main.Application.DomainEventHandlers.Product;

public class ProductStockUpdatedHandler(
    IIntegrationEventScope integrationEventScope
    ) : BatchableDomainEventHandler<ProductStockUpdatedDomainEvent>
{
    public override Task Handle(Batch<ProductStockUpdatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        foreach (var @event in notification.Items)
            integrationEventScope.Add(new ProductUpdatedEvent
            {
                Id = @event.Id
            });
            
        return Task.CompletedTask;
    }
}