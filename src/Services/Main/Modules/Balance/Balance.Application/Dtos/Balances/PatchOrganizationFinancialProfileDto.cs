using System.Text.Json.Serialization;
using Application.Common.Models;

namespace Main.Modules.Balance.Application.Dtos.Balances;

public record PatchOrganizationFinancialProfileDto
{
	[JsonPropertyName("minimalAllowedBalance")]
	public PatchField<decimal> MinimalAllowedBalance { get; init; } = PatchField<decimal>.NotSet();
}
