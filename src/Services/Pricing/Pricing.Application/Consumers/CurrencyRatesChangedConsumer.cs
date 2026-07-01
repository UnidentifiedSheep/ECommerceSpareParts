using Contracts.Analytics;
using Contracts.Currency;
using MassTransit;
using Pricing.Application.Interfaces.Cache;

namespace Pricing.Application.Consumers;

public class CurrencyRatesChangedConsumer(
    ICurrencyCacheRepository cacheRepository
) : IConsumer<CurrencyRateChangedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
    {
        foreach (var (id, _) in context.Message.Rates)
            await cacheRepository.InvalidateCurrencyRate(id, context.CancellationToken);

        await context.Publish(new MarkupRangesRefreshRequestedEvent(), context.CancellationToken);
    }
}