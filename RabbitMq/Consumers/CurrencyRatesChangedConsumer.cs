using Core.Contracts;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using MassTransit;

namespace RabbitMq.Consumers;

public class CurrencyRatesChangedConsumer(ICurrencyRepository currencyRepository, ICurrencyConverter currencyConverter)
    : IConsumer<CurrencyRateChangedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
    {
        var toUsdDict = await currencyRepository.GetCurrenciesToUsd();
        currencyConverter.LoadRates(toUsdDict);
    }
}