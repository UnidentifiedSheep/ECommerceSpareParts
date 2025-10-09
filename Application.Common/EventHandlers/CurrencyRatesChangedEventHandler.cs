using Core.Contracts;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.MessageBroker;

namespace Application.Common.EventHandlers;

public class CurrencyRatesChangedEventHandler(ICurrencyRepository currencyRepository, ICurrencyConverter currencyConverter)
    : IEventHandler<CurrencyRateChangedEvent>
{
    public async Task HandleAsync(IEventContext<CurrencyRateChangedEvent> context)
    {
        var toUsdDict = await currencyRepository.GetCurrenciesToUsd();
        currencyConverter.LoadRates(toUsdDict);
    }
}