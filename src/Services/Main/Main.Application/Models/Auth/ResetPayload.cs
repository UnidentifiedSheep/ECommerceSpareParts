using System.Text.Json.Serialization;
using Main.Enums.Auth;

namespace Main.Application.Models.Auth;

public record ResetPayload
{
    [JsonPropertyName("type")]
    public required ResetType Type { get; init; }

    [JsonPropertyName("userId")]
    public required Guid UserId { get; init; }

    [JsonPropertyName("expires")]
    public required DateTime Expires { get; init; }
}