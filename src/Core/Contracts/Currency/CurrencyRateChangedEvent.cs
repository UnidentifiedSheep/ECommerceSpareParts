namespace Contracts.Currency;

public record CurrencyRateChangedEvent
{
    public Dictionary<int, decimal> Rates { get; init; } = [];
}