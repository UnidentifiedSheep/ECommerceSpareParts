using Main.Modules.Currency.Abstractions;
using Pricing.Application.Events;
using MassTransit;

namespace Pricing.Application.Consumers;

public class PricingCurrencyRatesChangedConsumer : IConsumer<CurrencyRateChangedEvent>
{
	public Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
		=> context.Publish(new MarkupRangesRefreshRequestedEvent(), context.CancellationToken);
}
