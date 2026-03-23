using System.Text.Json.Serialization;

namespace Localization.Abstractions.Models;

public record LocalizeModel
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }
    [JsonPropertyName("value")]
    public required string Value { get; init; }
}