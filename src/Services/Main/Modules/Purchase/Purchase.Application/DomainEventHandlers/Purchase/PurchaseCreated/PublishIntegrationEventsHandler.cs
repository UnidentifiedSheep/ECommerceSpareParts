using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts;
using Domain.Events;
using PurchaseEntity = Main.Entities.Purchase.Purchase.Purchase;

namespace Main.Modules.Purchase.Application.DomainEventHandlers.Purchase.PurchaseCreated;

public sealed class PublishIntegrationEventsHandler(IIntegrationEventScope integrationEventScope)
	: BatchableDomainEventHandler<EntityCreatedDomainEvent<PurchaseEntity>>
{
	public override Task Handle(
		Batch<EntityCreatedDomainEvent<PurchaseEntity>> notification,
		CancellationToken cancellationToken)
	{
		integrationEventScope.AddRange(
			notification.Items.Select(item => new PurchaseCreatedEvent { Id = item.Entity.Id }));
		return Task.CompletedTask;
	}
}
