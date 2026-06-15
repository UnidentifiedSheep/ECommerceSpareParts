using System.Text.Json.Serialization;
using Main.Application.Dtos.Users;
using Main.Enums.Auth;

namespace Main.Application.Dtos.Auth;

public record UserPartyDto
{
    [JsonPropertyName("partyType")]
    public required UserPartyType PartyType { get; init; }
    
    [JsonPropertyName("user")]
    public UserDto? User { get; init; }
}