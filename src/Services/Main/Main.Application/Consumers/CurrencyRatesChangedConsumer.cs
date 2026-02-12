using Abstractions.Interfaces;
using Abstractions.Interfaces.Currency;
using Contracts.Currency;
using Main.Abstractions.Interfaces.DbRepositories;
using MassTransit;

namespace Main.Application.Consumers;

public class CurrencyRatesChangedConsumer(ICurrencyRepository currencyRepository, ICurrencyConverter currencyConverter)
    : IConsumer<CurrencyRateChangedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
    {
        var toUsdDict = await currencyRepository.GetCurrenciesToUsd();
        currencyConverter.LoadRates(toUsdDict);
    }
}