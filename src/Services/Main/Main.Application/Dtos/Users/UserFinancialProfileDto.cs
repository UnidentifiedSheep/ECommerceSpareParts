using System.Text.Json.Serialization;
using Main.Application.Dtos.Currencies;

namespace Main.Application.Dtos.Users;

public record UserFinancialProfileDto
{
    [JsonPropertyName("balance")]
    public required decimal Balance { get; init; }
    
    [JsonPropertyName("minimalAllowedBalance")]
    public required decimal MinimalAllowedBalance { get; init; }
}