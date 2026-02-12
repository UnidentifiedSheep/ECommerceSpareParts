namespace Abstractions.Models;

/// <summary>
/// 
/// </summary>
/// <param name="Base">Code of base currency. USD, RUB, AUD, etc.</param>
/// <param name="Rates">Dictionary of currency codes and exchange rates.</param>
public record ExchangeRates(string Base, Dictionary<string, decimal> Rates);