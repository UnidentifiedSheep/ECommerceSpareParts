using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Products;
using Main.Entities.DomainEvents.Product;

namespace Main.Application.DomainEventHandlers.Product;

public class ProductCreatedHandler(
    IIntegrationEventScope integrationEventScope
    ) : BatchableDomainEventHandler<ProductCreatedDomainEvent>
{
    public override Task Handle(Batch<ProductCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var events = notification.Items
            .Select(x => x.Product.Id)
            .Distinct()
            .Select(x => x == 0
                ? throw new InvalidOperationException("Product must have an Id before integration " +
                                                      "events are created. Call save changes before.")
                : new ProductUpdatedEvent() { Id = x }
            )
            .ToList();

        integrationEventScope.AddRange(events);
        return Task.CompletedTask;
    }
}