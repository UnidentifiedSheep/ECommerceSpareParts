using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Producer;
using Main.Entities.Product.DomainEvents.Producer;
using Main.Modules.Product.Application.Events;

namespace Main.Modules.Product.Application.DomainEventHandlers.Producer.ProducerCreated;

public class PublishIntegrationEventsHandler(IIntegrationEventScope integrationEventScope)
	: BatchableDomainEventHandler<ProducerCreatedDomainEvent>
{
	public override Task Handle(
		Batch<ProducerCreatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var events = notification
			.Items
			.Select(x => x.Producer.Id)
			.Distinct()
			.Select(x => x == 0
				? throw new InvalidOperationException(
					"Producer must have an Id before integration " +
					"events are created. Call save changes before.")
				: new ProducerUpdatedEvent
				{
					Id = x
				})
			.ToList();

		integrationEventScope.AddRange(events);
		integrationEventScope.Add(new ProducerLookupChangedEvent());
		return Task.CompletedTask;
	}
}
