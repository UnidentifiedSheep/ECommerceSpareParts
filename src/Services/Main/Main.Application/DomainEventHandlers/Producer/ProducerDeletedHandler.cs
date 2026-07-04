using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Producer;
using Main.Entities.DomainEvents.Producer;

namespace Main.Application.DomainEventHandlers.Producer;

public class ProducerDeletedHandler(
    IIntegrationEventScope integrationEventScope
    ) : BatchableDomainEventHandler<ProducerDeletedDomainEvent>
{
    public override Task Handle(Batch<ProducerDeletedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var events = notification.Items
            .Select(x => new ProducerUpdatedEvent
            {
                Id = x.ProducerId
            })
            .ToList();
        integrationEventScope.AddRange(events);
        return Task.CompletedTask;
    }
}