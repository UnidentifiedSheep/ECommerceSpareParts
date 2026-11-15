using System.Text.Json.Serialization;
using Main.Application.Dtos.Currencies;

namespace Main.Application.Dtos.Balances;

public record OrganizationBalanceDto
{
	[JsonPropertyName("balance")]
	public required decimal Balance { get; init; }

	[JsonPropertyName("currencyId")]
	public required int CurrencyId { get; init; }
}
