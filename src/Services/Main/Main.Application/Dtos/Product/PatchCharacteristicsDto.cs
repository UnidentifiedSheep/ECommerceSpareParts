using System.Text.Json.Serialization;
using Abstractions.Models;

namespace Main.Application.Dtos.Product;

public record PatchCharacteristicsDto
{
    [JsonPropertyName("value")]
    public PatchField<string> Value { get; init; } = PatchField<string>.NotSet();
}