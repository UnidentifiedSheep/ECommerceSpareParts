using System.Text.Json.Serialization;

namespace Main.Modules.Balance.Application.Dtos.Balances;

public record OrganizationFinancialProfileDto
{
	[JsonPropertyName("netPositionInBaseCurrency")]
	public required decimal NetPositionInBaseCurrency { get; init; }

	[JsonPropertyName("minimalAllowedBalance")]
	public required decimal MinimalAllowedBalance { get; init; }
}
