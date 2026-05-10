using Contracts.Currency;
using MassTransit;

namespace Analytics.Application.Consumers;

public class CurrencyRatesChangedConsumer()
    : IConsumer<CurrencyRateChangedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
    {
    }
}