using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Producer;
using Main.Entities.Product.DomainEvents.Producer;
using Main.Modules.Product.Application.Events;

namespace Main.Modules.Product.Application.DomainEventHandlers.Producer.ProducerAliasDeleted;

public class PublishIntegrationEventsHandler(IIntegrationEventScope integrationEventScope)
	: BatchableDomainEventHandler<ProducerAliasDeletedDomainEvent>
{
	public override Task Handle(
		Batch<ProducerAliasDeletedDomainEvent> notification,
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
