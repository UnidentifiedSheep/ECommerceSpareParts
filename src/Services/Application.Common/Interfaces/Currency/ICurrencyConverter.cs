using Abstractions.Models;

namespace Application.Common.Interfaces.Currency;

public interface ICurrencyConverter
{
    decimal Convert(decimal value, decimal fromRate, decimal toRate);

    decimal ToBase(decimal value, decimal fromRate);
    decimal FromBase(decimal value, decimal toRate);

    ExchangeRates ChangeBaseCurrency(ExchangeRates data, string newBase);
}