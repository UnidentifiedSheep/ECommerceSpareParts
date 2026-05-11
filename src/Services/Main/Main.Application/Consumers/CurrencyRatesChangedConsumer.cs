using Contracts.Currency;
using MassTransit;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application.Consumers;

public class CurrencyRatesChangedConsumer(IFusionCache cache)
    : IConsumer<CurrencyRateChangedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
    {
        var keys = context.Message.Rates.Keys.Select(x => $"currency:{x}");
        await cache.RemoveByTagAsync(keys);
    }
}