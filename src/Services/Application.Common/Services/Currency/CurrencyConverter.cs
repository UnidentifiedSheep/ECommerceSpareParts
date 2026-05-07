using Abstractions.Models;
using Application.Common.Interfaces.Currency;

namespace Application.Common.Services;

public class CurrencyConverter: ICurrencyConverter
{
    public decimal Convert(decimal value, decimal fromRate, decimal toRate)
    {
        if (fromRate == toRate)
            return value;

        var baseValue = value / fromRate;
        return baseValue * toRate;
    }

    public decimal ToBase(decimal value, decimal fromRate) => value / fromRate;

    public decimal FromBase(decimal value, decimal toRate) => value * toRate;

    public ExchangeRates ChangeBaseCurrency(ExchangeRates data, string newBase)
    {
        if (data.Base == newBase)
            return data;

        if (!data.Rates.TryGetValue(newBase, out var newBaseRate))
            throw new ArgumentException($"Валюта с кодом '{newBase}' не найдена.");

        var newRates = new Dictionary<string, decimal>
        {
            [data.Base] = 1 / newBaseRate
        };

        foreach (var (currency, rate) in data.Rates)
        {
            if (currency == newBase) continue;
            newRates[currency] = rate / newBaseRate;
        }

        return new ExchangeRates(newBase, newRates);
    }
}