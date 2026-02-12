using Abstractions.Interfaces.Currency;
using Contracts.Currency;
using MassTransit;
using Pricing.Abstractions.Interfaces.Services;

namespace Pricing.Application.Consumers;

public class CurrencyRatesChangedConsumer(ICurrencyConverterSetup currencyConverter)
    : IConsumer<CurrencyRateChangedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
    {
        await currencyConverter.InitializeAsync();
    }
}