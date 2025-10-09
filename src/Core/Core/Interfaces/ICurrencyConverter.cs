namespace Core.Interfaces;

public interface ICurrencyConverter
{
    Dictionary<int, decimal> ToUsd { get; }
    Dictionary<int, double> ToUsdDoub { get; }
    void LoadRates(Dictionary<int, decimal> rawRates);
    decimal ConvertTo(decimal value, int from, int to);
    double ConvertTo(double value, int from, int to);
    decimal ConvertToUsd(decimal value, int from);
    double ConvertToUsd(double value, int from);
    double ConvertFromUsd(double value, int to);
    decimal ConvertFromUsd(decimal value, int to);
    decimal GetRateDecimal(int currencyId);
    double GetRateDouble(int currencyId);
    bool IsSupportedCurrency(int currencyId);
}