using System.Text.Json.Serialization;
using Abstractions.Models;

namespace Main.Application.Dtos.Balances;

public record PatchOrganizationFinancialProfileDto
{
    [JsonPropertyName("minimalAllowedBalance")]
    public PatchField<decimal> MinimalAllowedBalance { get; init; } = PatchField<decimal>.NotSet();
}
