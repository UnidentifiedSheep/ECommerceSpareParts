using Abstractions.Interfaces.Currency;
using Main.Abstractions.Interfaces.DbRepositories;

namespace Main.Application.Services;

public class CurrencyConverterSetup(ICurrencyConverter currencyConverter, ICurrencyRepository currencyRepository) 
    : ICurrencyConverterSetup
{
    public async Task InitializeAsync()
    {
        var rates = await currencyRepository.GetCurrenciesToUsd();
        currencyConverter.LoadRates(rates);
    }
}