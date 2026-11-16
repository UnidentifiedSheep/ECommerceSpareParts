using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Domain.Events;
using Main.Entities.Currency;
using Main.Modules.Currency.Abstractions.Events;

namespace Main.Modules.Currency.Application.DomainEventHandlers.CurrencyRateCreated;

public sealed class PublishIntegrationEventsHandler(IIntegrationEventScope integrationEventScope)
	: BatchableDomainEventHandler<EntityCreatedDomainEvent<CurrencyRate>>
{
	public override Task Handle(
		Batch<EntityCreatedDomainEvent<CurrencyRate>> notification,
		CancellationToken cancellationToken)
	{
		integrationEventScope.Add(
			new CurrencyRateChangedEvent
			{
				Changes = notification
					.Items
					.Select(x => new CurrencyRateChange(
						x.Entity.FromCurrencyId,
						x.Entity.ToCurrencyId,
						x.Entity.Rate,
						x.Entity.CreatedAt))
					.ToList()
			});
		return Task.CompletedTask;
	}
}
