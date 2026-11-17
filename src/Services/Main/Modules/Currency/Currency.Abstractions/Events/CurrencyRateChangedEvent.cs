namespace Main.Modules.Currency.Abstractions.Events;

public record CurrencyRateChangedEvent
{
	public IReadOnlyList<CurrencyRateChange> Changes { get; init; } = [];
}

public record CurrencyRateChange(int CurrencyId,
	int BaseCurrencyId,
	decimal Rate,
	DateTime CreatedAt);
