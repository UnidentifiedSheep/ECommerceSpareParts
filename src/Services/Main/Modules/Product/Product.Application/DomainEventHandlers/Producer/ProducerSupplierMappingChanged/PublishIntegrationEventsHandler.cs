using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Main.Entities.Product.DomainEvents.Producer;
using Main.Modules.Product.Application.Events;

namespace Main.Modules.Product.Application.DomainEventHandlers.Producer.ProducerSupplierMappingChanged;

public sealed class PublishIntegrationEventsHandler(IIntegrationEventScope integrationEvents)
	: BatchableDomainEventHandler<ProducerSupplierMappingChangedDomainEvent>
{
	public override Task Handle(
		Batch<ProducerSupplierMappingChangedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		integrationEvents.Add(new ProducerLookupChangedEvent());
		return Task.CompletedTask;
	}
}
