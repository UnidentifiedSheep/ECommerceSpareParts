using Analytics.Application.Interfaces.Cache;
using Contracts.Currency;
using MassTransit;

namespace Analytics.Application.Consumers;

public class CurrencyRatesChangedConsumer(
    ICurrencyCacheRepository cacheRepository
) : IConsumer<CurrencyRateChangedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
    {
        foreach (var (id, _) in context.Message.Rates) await cacheRepository.InvalidateCurrencyRate(id);
    }
}