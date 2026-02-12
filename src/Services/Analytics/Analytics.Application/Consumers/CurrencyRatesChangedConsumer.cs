using Abstractions.Interfaces.Services;
using Analytics.Core.Entities;
using Analytics.Core.Interfaces.DbRepositories;
using Contracts.Currency;
using MassTransit;

namespace Analytics.Application.Consumers;

public class CurrencyRatesChangedConsumer(ICurrencyRepository currencyRepository, IUnitOfWork unitOfWork) : IConsumer<CurrencyRateChangedEvent>
{
    public async Task Consume(ConsumeContext<CurrencyRateChangedEvent> context)
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