using System.Text.Json.Serialization;
using Main.Enums;

namespace Main.Application.Dtos.Users;

public record UserPhoneDto
{
    [JsonPropertyName("number")]
    public required string Number { get; init; }

    [JsonPropertyName("type")]
    public required PhoneType Type { get; init; }

    [JsonPropertyName("isConfirmed")]
    public bool IsConfirmed { get; init; }

    [JsonPropertyName("isPrimary")]
    public bool IsPrimary { get; init; }
}