using Core.Interfaces;

namespace Application.Pricing;

public class CurrencyConverter(int usdId) : ICurrencyConverter
{
    private readonly Dictionary<int, decimal> _toUsdDecimal = new();
    private readonly Dictionary<int, double> _toUsdDouble = new();

    public Dictionary<int, decimal> ToUsd => _toUsdDecimal;
    public Dictionary<int, double> ToUsdDoub => _toUsdDouble;

    public void LoadRates(Dictionary<int, decimal> rawRates)
    {
        _toUsdDecimal.Clear();
        _toUsdDouble.Clear();

        foreach (var (currencyId, rate) in rawRates)
        {
            _toUsdDecimal[currencyId] = rate;
            _toUsdDouble[currencyId] = (double)rate;
        }
    }

    // === DECIMAL API ===

    public decimal ConvertTo(decimal value, int from, int to)
    {
        if (from == to) return value;

        if (!_toUsdDecimal.ContainsKey(from))
            throw new KeyNotFoundException($"Rate not found for currencyId {from}");
        if (!_toUsdDecimal.ContainsKey(to))
            throw new KeyNotFoundException($"Rate not found for currencyId {to}");

        var usd = value / _toUsdDecimal[from];
        return usd * _toUsdDecimal[to];
    }

    public decimal ConvertToUsd(decimal value, int from) =>
        ConvertTo(value, from, usdId);

    public decimal ConvertFromUsd(decimal value, int to)
        => ConvertTo(value, usdId, to);

    // === DOUBLE API ===

    public double ConvertTo(double value, int from, int to)
    {
        if (from == to) return value;

        if (!_toUsdDouble.ContainsKey(from))
            throw new KeyNotFoundException($"Rate not found for currencyId {from}");
        if (!_toUsdDouble.ContainsKey(to))
            throw new KeyNotFoundException($"Rate not found for currencyId {to}");

        var usd = value / _toUsdDouble[from];
        return usd * _toUsdDouble[to];
    }

    public double ConvertToUsd(double value, int from) =>
        ConvertTo(value, from, usdId);

    public double ConvertFromUsd(double value, int to)
        => ConvertTo(value, usdId, to);

    // === Accessors ===

    public decimal GetRateDecimal(int currencyId) =>
        _toUsdDecimal.TryGetValue(currencyId, out var rate)
            ? rate
            : throw new KeyNotFoundException($"Rate not found for currencyId {currencyId}");

    public double GetRateDouble(int currencyId) =>
        _toUsdDouble.TryGetValue(currencyId, out var rate)
            ? rate
            : throw new KeyNotFoundException($"Rate not found for currencyId {currencyId}");

    public bool IsSupportedCurrency(int currencyId) => ToUsd.ContainsKey(currencyId);
}