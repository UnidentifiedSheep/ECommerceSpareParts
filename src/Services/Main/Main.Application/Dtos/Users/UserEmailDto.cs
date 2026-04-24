using System.Text.Json.Serialization;
using Main.Enums;

namespace Main.Application.Dtos.Users;

public record UserEmailDto
{
    [JsonPropertyName("email")]
    public required string Email { get; init; } = null!;
    
    [JsonPropertyName("confirmed")]
    public required bool Confirmed { get; init; }
    
    [JsonPropertyName("emailType")]
    [JsonConverter(typeof(JsonStringEnumConverter<EmailType>))]
    public required EmailType EmailType { get; init; }
    
    [JsonPropertyName("isPrimary")]
    public required bool IsPrimary { get; init; }
    
    [JsonPropertyName("confirmedAt")]
    public required DateTime? ConfirmedAt { get; init; }
    
    [JsonPropertyName("createdAt")]
    public required DateTime CreatedAt { get; init; }
    
    [JsonPropertyName("updatedAt")]
    public required DateTime UpdatedAt { get; init; }
}