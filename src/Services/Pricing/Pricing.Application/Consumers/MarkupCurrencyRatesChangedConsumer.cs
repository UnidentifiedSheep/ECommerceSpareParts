using Contracts.Currency;
using MassTransit;
using Pricing.Application.Interfaces;

namespace Pricing.Application.Consumers;

public class MarkupCurrencyRatesChangedConsumer(
    IMarkupInitializer markupInitializer) : IConsumer<CurrencyRateChangedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
    {
        await markupInitializer.Initialize(context.CancellationToken);
    }
}
