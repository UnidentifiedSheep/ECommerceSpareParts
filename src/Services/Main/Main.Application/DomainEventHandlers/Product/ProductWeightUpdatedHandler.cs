using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Products;
using Main.Entities.DomainEvents.Product;

namespace Main.Application.DomainEventHandlers.Product;

public class ProductWeightUpdatedHandler(IIntegrationEventScope integrationEventScope)
	: BatchableDomainEventHandler<ProductWeightUpdatedDomainEvent>
{
	public override Task Handle(
		Batch<ProductWeightUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		foreach (var i in notification.Items)
			integrationEventScope.Add(
				new ProductUpdatedEvent
				{
					Id = i.ProductId
				});

		return Task.CompletedTask;
	}
}
