using Abstractions.Interfaces.Currency;
using Contracts.Currency;
using MassTransit;

namespace Main.Application.Consumers;

public class CurrencyRatesChangedConsumer(ICurrencyConverterSetup converterSetup)
    : IConsumer<CurrencyRateChangedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
    {
        await converterSetup.InitializeAsync();
    }
}