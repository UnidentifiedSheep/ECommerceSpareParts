using System.Text.Json.Serialization;
using Main.Application.Dtos.Currencies;

namespace Main.Application.Dtos.Balances;

public record UserBalanceDto
{
    [JsonPropertyName("balance")]
    public required decimal Balance { get; init; }

    [JsonPropertyName("currency")]
    public required CurrencyDto Currency { get; init; }
}