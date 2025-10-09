using Core.Interfaces;

namespace Application.Common;

public class CurrencyConverter(int usdId) : ICurrencyConverter
{
    public Dictionary<int, decimal> ToUsd { get; } = new();

    public Dictionary<int, double> ToUsdDoub { get; } = new();

    public void LoadRates(Dictionary<int, decimal> rawRates)
    {
        ToUsd.Clear();
        ToUsdDoub.Clear();

        foreach (var (currencyId, rate) in rawRates)
        {
            ToUsd[currencyId] = rate;
            ToUsdDoub[currencyId] = (double)rate;
        }
    }

    // === DECIMAL API ===

    public decimal ConvertTo(decimal value, int from, int to)
    {
        if (from == to) return value;

        if (!ToUsd.ContainsKey(from))
            throw new KeyNotFoundException($"Rate not found for currencyId {from}");
        if (!ToUsd.ContainsKey(to))
            throw new KeyNotFoundException($"Rate not found for currencyId {to}");

        var usd = value / ToUsd[from];
        return usd * ToUsd[to];
    }

    public decimal ConvertToUsd(decimal value, int from)
    {
        return ConvertTo(value, from, usdId);
    }

    public decimal ConvertFromUsd(decimal value, int to)
    {
        return ConvertTo(value, usdId, to);
    }

    // === DOUBLE API ===

    public double ConvertTo(double value, int from, int to)
    {
        if (from == to) return value;

        if (!ToUsdDoub.ContainsKey(from))
            throw new KeyNotFoundException($"Rate not found for currencyId {from}");
        if (!ToUsdDoub.ContainsKey(to))
            throw new KeyNotFoundException($"Rate not found for currencyId {to}");

        var usd = value / ToUsdDoub[from];
        return usd * ToUsdDoub[to];
    }

    public double ConvertToUsd(double value, int from)
    {
        return ConvertTo(value, from, usdId);
    }

    public double ConvertFromUsd(double value, int to)
    {
        return ConvertTo(value, usdId, to);
    }

    // === Accessors ===

    public decimal GetRateDecimal(int currencyId)
    {
        return ToUsd.TryGetValue(currencyId, out var rate)
            ? rate
            : throw new KeyNotFoundException($"Rate not found for currencyId {currencyId}");
    }

    public double GetRateDouble(int currencyId)
    {
        return ToUsdDoub.TryGetValue(currencyId, out var rate)
            ? rate
            : throw new KeyNotFoundException($"Rate not found for currencyId {currencyId}");
    }

    public bool IsSupportedCurrency(int currencyId)
    {
        return ToUsd.ContainsKey(currencyId);
    }
}