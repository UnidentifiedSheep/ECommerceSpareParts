using System.Text.Json.Serialization;
using Main.Enums.Auth;

namespace Main.Application.Models.Auth;

public record ResetPayload
{
    [JsonPropertyName("id")]
    public Guid Id { get; } = Guid.NewGuid();
    
    [JsonPropertyName("type")]
    public required ResetType Type { get; init; }

    [JsonPropertyName("userId")]
    public required Guid UserId { get; init; }
}