using Main.Application.Interfaces.Services.Currency;
using Main.Modules.Currency.Abstractions;
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
