namespace Main.Modules.Currency.Abstractions;

public record CurrencyRateChangedEvent
{
	public Dictionary<int, decimal> Rates { get; init; } = [];
}
