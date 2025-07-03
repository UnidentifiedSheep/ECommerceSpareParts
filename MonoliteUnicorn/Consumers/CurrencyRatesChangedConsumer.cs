using Core.RabbitMq.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Prices.PriceGenerator;

namespace MonoliteUnicorn.Consumers;

public class CurrencyRatesChangedConsumer(DContext dContext) : IConsumer<CurrencyRateChangedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
    {
        var toUsdDict = await dContext.CurrencyToUsds.AsNoTracking()
            .ToDictionaryAsync(x => x.CurrencyId, x => x.ToUsd);
        CurrencyConverter.LoadRates(toUsdDict);
    }
}