using Analytics.Core.Entities;
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
        var currencies = (await currencyRepository.GetCurrencies(rates.Keys)).ToList();

        if (currencies.Count != rates.Count)
            await CreateNotExistingCurrencies(rates, currencies);
        
        foreach (var currency in currencies)
            currency.ToUsd = rates[currency.Id];
        await unitOfWork.SaveChangesAsync();
    }

    private async Task CreateNotExistingCurrencies(Dictionary<int, decimal> rates, List<Currency> currencies)
    {
        var notExisting = rates.Keys.Except(currencies.Select(x => x.Id))
            .Select(x => new Currency
            {
                Id = x,
            }).ToList();
        await unitOfWork.AddRangeAsync(notExisting);
        foreach (var curr in notExisting)
            currencies[curr.Id] = curr;
    }
}