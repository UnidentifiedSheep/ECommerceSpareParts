using System.Text.Json.Serialization;

namespace Main.Abstractions.Dtos.Users;

public record UserInfoDto
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("surname")]
    public required string Surname { get; init; }
    
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}