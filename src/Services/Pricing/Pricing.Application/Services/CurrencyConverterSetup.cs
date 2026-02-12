using Abstractions.Interfaces.Currency;
using Pricing.Abstractions.Interfaces.Services;

namespace Pricing.Application.Services;

public class CurrencyConverterSetup(ICurrencyConverter currencyConverter, ICurrencyService currencyService) 
    : ICurrencyConverterSetup
{
    public async Task InitializeAsync()
    {
        var rates = (await currencyService.GetCurrencies())
            .ToDictionary(x => x.Id, x => x.ToUsdRate);
        currencyConverter.LoadRates(rates);
    }
}