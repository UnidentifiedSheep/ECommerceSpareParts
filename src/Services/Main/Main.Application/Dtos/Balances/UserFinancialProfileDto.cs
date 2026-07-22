using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Balances;

public record UserFinancialProfileDto
{
    [JsonPropertyName("netPositionInBaseCurrency")]
    public required decimal NetPositionInBaseCurrency { get; init; }

    [JsonPropertyName("minimalAllowedBalance")]
    public required decimal MinimalAllowedBalance { get; init; }
}
