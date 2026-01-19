using Contracts.Currency;
using Core.Interfaces;
using Core.Interfaces.MessageBroker;
using Main.Abstractions.Interfaces.DbRepositories;

namespace Main.Application.EventHandlers;

public class CurrencyRatesChangedEventHandler(
    ICurrencyRepository currencyRepository,
    ICurrencyConverter currencyConverter)
    : IEventHandler<CurrencyRateChangedEvent>
{
    public async Task HandleAsync(IEventContext<CurrencyRateChangedEvent> context)
    {
        var toUsdDict = await currencyRepository.GetCurrenciesToUsd();
        currencyConverter.LoadRates(toUsdDict);
    }
}