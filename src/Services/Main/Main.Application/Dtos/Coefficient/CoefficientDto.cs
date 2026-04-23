using System.Text.Json.Serialization;
using Enums;

namespace Main.Application.Dtos.Amw.Coefficients;

public record CoefficientDto
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("value")]
    public required decimal Value { get; init; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<CoefficientType>))]
    public required CoefficientType Type { get; init; }
}