using System.Text.Json.Serialization;
using Main.Application.Dtos.Users;
using Main.Enums.Balances;

namespace Main.Application.Dtos.Balances;

public record TransactionPartyDto
{
    [JsonPropertyName("partyType")]
    public required TransactionPartyType PartyType { get; init; }
    
    [JsonPropertyName("user")]
    public UserDto? User { get; init; }
}