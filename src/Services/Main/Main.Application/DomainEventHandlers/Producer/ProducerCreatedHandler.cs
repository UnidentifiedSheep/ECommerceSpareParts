using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Producer;
using Main.Entities.DomainEvents.Producer;

namespace Main.Application.DomainEventHandlers.Producer;

public class ProducerCreatedHandler(
    IIntegrationEventScope integrationEventScope
    ) : BatchableDomainEventHandler<ProducerCreatedDomainEvent>
{
    public override Task Handle(Batch<ProducerCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var events = notification.Items
            .Select(x => x.Producer.Id)
            .Distinct()
            .Select(x => x == 0
                ? throw new InvalidOperationException("Producer must have an Id before integration " +
                                                      "events are created. Call save changes before.")
                : new ProducerUpdatedEvent { Id = x }
            )
            .ToList();

        integrationEventScope.AddRange(events);
        return Task.CompletedTask;
    }
}