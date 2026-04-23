using System.Text.Json.Serialization;
using Abstractions.Models;

namespace Main.Application.Dtos.Amw.ArticleCharacteristics;

public record PatchCharacteristicsDto
{
    [JsonPropertyName("value")]
    public PatchField<string> Value { get; init; } = PatchField<string>.NotSet();
}