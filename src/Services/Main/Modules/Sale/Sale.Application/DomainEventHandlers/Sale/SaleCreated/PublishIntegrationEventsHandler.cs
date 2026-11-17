using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts;
using Domain.Events;
using SaleEntity = Main.Entities.Sale.Sale.Sale;

namespace Main.Modules.Sale.Application.DomainEventHandlers.Sale.SaleCreated;

public sealed class PublishIntegrationEventsHandler(IIntegrationEventScope integrationEventScope)
	: BatchableDomainEventHandler<EntityCreatedDomainEvent<SaleEntity>>
{
	public override Task Handle(
		Batch<EntityCreatedDomainEvent<SaleEntity>> notification,
		CancellationToken cancellationToken)
	{
		integrationEventScope.AddRange(
			notification.Items.Select(item => new SaleCreatedEvent { Id = item.Entity.Id }));
		return Task.CompletedTask;
	}
}
