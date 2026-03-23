using System.Text.Json.Serialization;

namespace Abstractions.Models.Localization;

public record LocaleFullInfoModel
{
    [JsonPropertyName("locale")]
    public required string Locale { get; init; }

    [JsonPropertyName("keyValues")] 
    public IReadOnlyList<LocalizeModel> KeyValues { get; init; } = [];
}