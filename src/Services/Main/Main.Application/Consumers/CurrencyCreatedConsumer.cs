using Application.Common.Interfaces;
using Contracts.Currency;
using Main.Application.Handlers.Currencies.GetCurrencies;
using MassTransit;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application.Consumers;

public class CurrencyCreatedConsumer(
    IFusionCache cache,
    ICachePolicy<GetCurrenciesQuery> cachePolicy) : IConsumer<CurrencyCreatedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyCreatedEvent> context)
    {
        if (cachePolicy.BaseTag != null)
            await cache.RemoveByTagAsync(cachePolicy.BaseTag);
    }
}