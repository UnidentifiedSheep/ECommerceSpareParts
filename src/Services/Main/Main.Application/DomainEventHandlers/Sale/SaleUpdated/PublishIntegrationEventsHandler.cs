using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts;
using Domain.Events;
using SaleEntity = Main.Entities.Sale.Sale;

namespace Main.Application.DomainEventHandlers.Sale.SaleUpdated;

public sealed class PublishIntegrationEventsHandler(IIntegrationEventScope integrationEventScope)
	: BatchableDomainEventHandler<EntityUpdatedDomainEvent<SaleEntity, Guid>>
{
	public override Task Handle(Batch<EntityUpdatedDomainEvent<SaleEntity, Guid>> notification, CancellationToken cancellationToken)
	{
		integrationEventScope.AddRange(notification.Items.Select(item => new SaleUpdatedEvent { Id = item.Id }));
		return Task.CompletedTask;
	}
}
