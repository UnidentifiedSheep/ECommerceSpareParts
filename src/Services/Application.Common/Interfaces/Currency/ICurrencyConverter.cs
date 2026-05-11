using Abstractions.Models;

namespace Application.Common.Interfaces.Currency;

public interface ICurrencyConverter
{
    decimal Convert(decimal value, decimal fromRate, decimal toRate);

    decimal ToBase(decimal value, decimal fromRate);
    decimal FromBase(decimal value, decimal toRate);

    Task<decimal> ConvertAsync(
        decimal value,
        int fromCurrencyId,
        int toCurrencyId,
        CancellationToken cancellationToken = default);

    Task<decimal> ConvertFromBaseAsync(
        decimal value,
        int toCurrencyId,
        CancellationToken cancellationToken = default);

    Task<decimal> ConvertToBaseAsync(
        decimal value,
        int fromCurrencyId,
        CancellationToken cancellationToken = default);

    ExchangeRates ChangeBaseCurrency(ExchangeRates data, string newBase);
}