namespace Core.Models.ExchangeRates;

public record LatestCurrencyRatesResponse(string BaseCurrency, string Date, Dictionary<string, decimal> Rates, string Timestamp);