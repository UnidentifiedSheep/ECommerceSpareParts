using Core.Interfaces;

namespace Contracts.Currency;

public record CurrencyRateChangedEvent(Dictionary<int, decimal> Rates) : IContract;