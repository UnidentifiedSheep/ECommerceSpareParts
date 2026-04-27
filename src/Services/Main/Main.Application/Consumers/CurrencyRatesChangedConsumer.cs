using Abstractions.Interfaces.Currency;
using Application.Common.Interfaces;
using Contracts.Currency;
using Main.Entities.Currency;
using MassTransit;

namespace Main.Application.Consumers;

public class CurrencyRatesChangedConsumer(
    ICurrencyConverterSetup converterSetup,
    ICacheInvalidator<Currency, int> cacheInvalidator)
    : IConsumer<CurrencyRateChangedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
    {
        await converterSetup.InitializeAsync();

        await cacheInvalidator.Invalidate(context.Message.Rates.Keys);
    }
}