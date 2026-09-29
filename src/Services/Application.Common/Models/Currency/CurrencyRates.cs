namespace Application.Common.Models.Currency;

public sealed record CurrencyRates(string BaseCurrencyCode, IReadOnlyDictionary<string, decimal> Rates);
