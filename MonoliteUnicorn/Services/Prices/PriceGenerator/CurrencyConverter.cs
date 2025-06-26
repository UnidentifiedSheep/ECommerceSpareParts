namespace MonoliteUnicorn.Services.Prices.PriceGenerator;

public static class CurrencyConverter
{
    private static readonly Dictionary<int, decimal> ToUsdDecimal = new();
    private static readonly Dictionary<int, double> ToUsdDouble = new();

    public static Dictionary<int, decimal> ToUsd => ToUsdDecimal;

    public static void LoadRates(Dictionary<int, decimal> rawRates)
    {
        ToUsdDecimal.Clear();
        ToUsdDouble.Clear();

        foreach (var (currencyId, rate) in rawRates)
        {
            ToUsdDecimal[currencyId] = rate;
            ToUsdDouble[currencyId] = (double)rate;
        }
    }

    // === DECIMAL API ===

    public static decimal ConvertTo(decimal value, int from, int to)
    {
        if (from == to) return value;

        if (!ToUsdDecimal.ContainsKey(from))
            throw new KeyNotFoundException($"Rate not found for currencyId {from}");
        if (!ToUsdDecimal.ContainsKey(to))
            throw new KeyNotFoundException($"Rate not found for currencyId {to}");

        var usd = value / ToUsdDecimal[from];
        return usd * ToUsdDecimal[to];
    }

    public static decimal ConvertToUsd(decimal value, int from) =>
        ConvertTo(value, from, Global.UsdId);

    // === DOUBLE API ===

    public static double ConvertTo(double value, int from, int to)
    {
        if (from == to) return value;

        if (!ToUsdDouble.ContainsKey(from))
            throw new KeyNotFoundException($"Rate not found for currencyId {from}");
        if (!ToUsdDouble.ContainsKey(to))
            throw new KeyNotFoundException($"Rate not found for currencyId {to}");

        var usd = value / ToUsdDouble[from];
        return usd * ToUsdDouble[to];
    }

    public static double ConvertToUsd(double value, int from) =>
        ConvertTo(value, from, Global.UsdId);

    // === Accessors ===

    public static decimal GetRateDecimal(int currencyId) =>
        ToUsdDecimal.TryGetValue(currencyId, out var rate)
            ? rate
            : throw new KeyNotFoundException($"Rate not found for currencyId {currencyId}");

    public static double GetRateDouble(int currencyId) =>
        ToUsdDouble.TryGetValue(currencyId, out var rate)
            ? rate
            : throw new KeyNotFoundException($"Rate not found for currencyId {currencyId}");
}