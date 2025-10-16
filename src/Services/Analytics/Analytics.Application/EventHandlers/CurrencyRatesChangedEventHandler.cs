using Analytics.Core.Interfaces.DbRepositories;
using Contracts.Currency;
using Core.Interfaces.MessageBroker;
using Core.Interfaces.Services;

namespace Analytics.Application.EventHandlers;

public class CurrencyRatesChangedEventHandler(ICurrencyRepository currencyRepository, IUnitOfWork unitOfWork) : IEventHandler<CurrencyRateChangedEvent>
{
    public async Task HandleAsync(IEventContext<CurrencyRateChangedEvent> context)
    {
        var rates = context.Message.Rates;
        var currencies = await currencyRepository.GetCurrencies(rates.Keys);
        foreach (var currency in currencies)
            currency.ToUsd = rates[currency.Id];
        await unitOfWork.SaveChangesAsync();
    }
}