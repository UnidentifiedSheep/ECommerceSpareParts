using Abstractions.Interfaces.Currency;
using Analytics.Core.Interfaces.DbRepositories;

namespace Analytics.Application.Services;

public class CurrencyConverterSetup(ICurrencyConverter currencyConverter, ICurrencyRepository currencyRepository) 
    : ICurrencyConverterSetup
{
    public async Task InitializeAsync()
    {
        var rates = (await currencyRepository.GetAllCurrencies(false))
            .ToDictionary(x => x.Id, x => x.ToUsd);
        
        currencyConverter.LoadRates(rates);
    }
}