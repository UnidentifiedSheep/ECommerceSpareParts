namespace Contracts.Currency.GetCurrencies;

public record GetCurrenciesResponse
{
    public List<Models.Currency.Currency> Currencies { get; init; } = null!;
}