using System.Text.Json.Serialization;

namespace Localization.Abstractions.Models;

public record LocaleFullInfoModel
{
    [JsonPropertyName("locale")]
    public required string Locale { get; init; }

    [JsonPropertyName("keyValues")]
    public Dictionary<string, string> KeyValues { get; init; } = [];
}