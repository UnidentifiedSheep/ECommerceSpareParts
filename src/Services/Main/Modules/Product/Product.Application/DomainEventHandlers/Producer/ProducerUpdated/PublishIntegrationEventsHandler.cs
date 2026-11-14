using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Producer;
using Main.Entities.DomainEvents.Producer;

namespace Main.Application.DomainEventHandlers.Producer.ProducerUpdated;

public class PublishIntegrationEventsHandler(IIntegrationEventScope integrationEventScope)
	: BatchableDomainEventHandler<ProducerUpdatedDomainEvent>
{
	public override Task Handle(
		Batch<ProducerUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var events = notification
			.Items
			.Select(x => new ProducerUpdatedEvent
			{
				Id = x.ProducerId
			})
			.ToList();
		integrationEventScope.AddRange(events);
		integrationEventScope.Add(new ProducerLookupChangedEvent());
		return Task.CompletedTask;
	}
}
