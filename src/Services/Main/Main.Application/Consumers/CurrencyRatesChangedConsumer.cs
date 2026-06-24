using Contracts.Currency;
using Main.Application.Interfaces.Cache;
using MassTransit;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application.Consumers;

public class CurrencyRatesChangedConsumer(ICurrencyCacheRepository currencyCacheRepository)
    : IConsumer<CurrencyRateChangedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
    {
        foreach (var (id, _) in context.Message.Rates)
            await currencyCacheRepository.InvalidateCurrencyRate(id);
    }
}