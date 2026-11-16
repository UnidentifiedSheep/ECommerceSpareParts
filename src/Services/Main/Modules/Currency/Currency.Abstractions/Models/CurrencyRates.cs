namespace Main.Modules.Currency.Abstractions.Models;

public sealed record CurrencyRates(string BaseCurrencyCode, IReadOnlyDictionary<string, decimal> Rates);
