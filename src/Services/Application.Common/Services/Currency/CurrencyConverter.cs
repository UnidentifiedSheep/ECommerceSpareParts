using Application.Common.Interfaces.Currency;

namespace Application.Common.Services.Currency;

public class CurrencyConverter(
    ICurrencyRatesProvider ratesProvider) : CurrencyConverterBase
{
    public override async Task<decimal> ConvertAsync(
        decimal value,
        int fromCurrencyId,
        int toCurrencyId,
        CancellationToken cancellationToken = default)
    {
        var fromRate = await ratesProvider.GetRate(fromCurrencyId, cancellationToken);
        var toRate = await ratesProvider.GetRate(toCurrencyId, cancellationToken);
        return Convert(value, fromRate, toRate);
    }

    public override async Task<decimal> ConvertFromBaseAsync(
        decimal value,
        int toCurrencyId,
        CancellationToken cancellationToken = default)
    {
        var rate = await ratesProvider.GetRate(toCurrencyId, cancellationToken);
        return FromBase(value, rate);
    }

    public override async Task<decimal> ConvertToBaseAsync(
        decimal value,
        int fromCurrencyId,
        CancellationToken cancellationToken = default)
    {
        var rate = await ratesProvider.GetRate(fromCurrencyId, cancellationToken);
        return ToBase(value, rate);
    }
}