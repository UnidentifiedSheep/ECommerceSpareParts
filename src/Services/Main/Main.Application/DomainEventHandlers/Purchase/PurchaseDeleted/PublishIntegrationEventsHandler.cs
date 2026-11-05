using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts;
using Domain.Events;
using PurchaseEntity = Main.Entities.Purchase.Purchase;

namespace Main.Application.DomainEventHandlers.Purchase.PurchaseDeleted;

public sealed class PublishIntegrationEventsHandler(IIntegrationEventScope integrationEventScope)
	: BatchableDomainEventHandler<EntityDeletedDomainEvent<PurchaseEntity, Guid>>
{
	public override Task Handle(Batch<EntityDeletedDomainEvent<PurchaseEntity, Guid>> notification, CancellationToken cancellationToken)
	{
		integrationEventScope.AddRange(notification.Items.Select(item => new PurchaseDeletedEvent { Id = item.Id }));
		return Task.CompletedTask;
	}
}
