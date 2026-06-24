using Contracts.Currency;
using Main.Application.Interfaces.Cache;
using MassTransit;

namespace Main.Application.Consumers;

public class CurrencyCreatedConsumer(
    ICurrencyCacheRepository cacheRepository) : IConsumer<CurrencyCreatedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyCreatedEvent> context)
    {
        await cacheRepository.InvalidateAllCurrencies();
    }
}