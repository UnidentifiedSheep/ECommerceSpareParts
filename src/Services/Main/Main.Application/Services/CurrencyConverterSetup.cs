using Abstractions.Interfaces.Currency;
using Main.Application.Handlers.Currencies.GetAllCurrencies;
using MediatR;

namespace Main.Application.Services;

public class CurrencyConverterSetup(
    ICurrencyConverter currencyConverter, 
    ISender sender)
    : ICurrencyConverterSetup
{
    public async Task InitializeAsync()
    {
        var rates = (await sender.Send(new GetAllCurrenciesQuery()))
            .Currencies
            .ToDictionary(x => x.Id, x => x.ToUsdRate ?? 0);
        currencyConverter.LoadRates(rates);
    }
}