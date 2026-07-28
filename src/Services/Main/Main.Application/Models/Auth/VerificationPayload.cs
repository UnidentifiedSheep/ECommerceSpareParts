using System.Text.Json.Serialization;
using Main.Enums.Auth;

namespace Main.Application.Models.Auth;

public record VerificationPayload
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; } = Guid.NewGuid();

    [JsonPropertyName("type")]
    public required VerificationType Type { get; init; }

    [JsonPropertyName("userId")]
    public required Guid UserId { get; init; }
    
    [JsonPropertyName("dataToVerify")]
    public required string DataToVerify { get; init; }
}
