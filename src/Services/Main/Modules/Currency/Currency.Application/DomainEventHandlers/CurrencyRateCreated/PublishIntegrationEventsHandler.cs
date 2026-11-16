using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Domain.Events;
using Main.Entities.Currency;
using Main.Modules.Currency.Abstractions;

namespace Main.Application.DomainEventHandlers.CurrencyRateCreated;

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
				Rates = notification
					.Items
					.GroupBy(x => x.Entity.FromCurrencyId)
					.ToDictionary(x => x.Key, x => x.Last().Entity.Rate)
			});
		return Task.CompletedTask;
	}
}
