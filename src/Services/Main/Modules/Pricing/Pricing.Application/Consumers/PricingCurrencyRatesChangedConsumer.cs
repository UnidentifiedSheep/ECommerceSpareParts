using Main.Modules.Currency.Abstractions.Events;
using Main.Modules.Currency.Abstractions.Interfaces;
using MassTransit;
using Pricing.Application.Interfaces.Markup;

namespace Pricing.Application.Consumers;

public class PricingCurrencyRatesChangedConsumer(
	ICurrencyRateSnapshotProvider rates,
	IMarkupRefresher markupRefresher) : IConsumer<CurrencyRateChangedEvent>
{
	public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
	{
		await rates.Apply(context.Message.Changes, context.CancellationToken);
		await markupRefresher.Refresh(context.CancellationToken);
	}
}
