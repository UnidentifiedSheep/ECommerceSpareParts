using System.Text.Json.Serialization;

namespace Main.Application.Models.Auth;

public record ResetPayload
{
    [JsonPropertyName("userId")]
    public required Guid UserId { get; init; }
    
    [JsonPropertyName("email")]
    public required string Email { get; init; }
    
    [JsonPropertyName("expires")]
    public required DateTime Expires { get; init; }
}