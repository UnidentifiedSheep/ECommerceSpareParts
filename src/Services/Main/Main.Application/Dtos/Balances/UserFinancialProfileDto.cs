using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Balances;

public record UserFinancialProfileDto
{
    [JsonPropertyName("balance")]
    public required decimal Balance { get; init; }
    
    [JsonPropertyName("minimalAllowedBalance")]
    public required decimal MinimalAllowedBalance { get; init; }
}