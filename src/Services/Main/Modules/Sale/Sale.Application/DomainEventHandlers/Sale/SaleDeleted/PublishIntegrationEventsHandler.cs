using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts;
using Domain.Events;
using SaleEntity = Main.Entities.Sale.Sale.Sale;

namespace Main.Modules.Sale.Application.DomainEventHandlers.Sale.SaleDeleted;

public sealed class PublishIntegrationEventsHandler(IIntegrationEventScope integrationEventScope)
	: BatchableDomainEventHandler<EntityDeletedDomainEvent<SaleEntity.Sale, Guid>>
{
	public override Task Handle(
		Batch<EntityDeletedDomainEvent<SaleEntity, Guid>> notification,
		CancellationToken cancellationToken)
	{
		integrationEventScope.AddRange(
			notification.Items.Select(item => new SaleDeletedEvent { Id = item.Id }));
		return Task.CompletedTask;
	}
}
