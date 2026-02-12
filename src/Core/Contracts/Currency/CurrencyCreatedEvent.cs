namespace Contracts.Currency;

public record CurrencyCreatedEvent
{
    public Models.Currency.Currency Currency { get; init; } = null!;

}