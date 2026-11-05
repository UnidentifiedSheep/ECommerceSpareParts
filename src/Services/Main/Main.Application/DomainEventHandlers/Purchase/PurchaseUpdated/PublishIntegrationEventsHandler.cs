using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts;
using Domain.Events;
using PurchaseEntity = Main.Entities.Purchase.Purchase;

namespace Main.Application.DomainEventHandlers.Purchase.PurchaseUpdated;

public sealed class PublishIntegrationEventsHandler(IIntegrationEventScope integrationEventScope)
	: BatchableDomainEventHandler<EntityUpdatedDomainEvent<PurchaseEntity, Guid>>
{
	public override Task Handle(Batch<EntityUpdatedDomainEvent<PurchaseEntity, Guid>> notification, CancellationToken cancellationToken)
	{
		integrationEventScope.AddRange(notification.Items.Select(item => new PurchaseUpdatedEvent { Id = item.Id }));
		return Task.CompletedTask;
	}
}
