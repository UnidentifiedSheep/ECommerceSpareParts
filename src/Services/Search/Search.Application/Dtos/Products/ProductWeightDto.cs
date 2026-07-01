using System.Text.Json.Serialization;

namespace Search.Application.Dtos.Products;

public record ProductWeightDto
{
    [JsonPropertyName("value")]
    public required decimal Value { get; init; }

    [JsonPropertyName("unit")]
    public required string Unit { get; init; }

    [JsonPropertyName("weightKg")]
    public required decimal WeightKg { get; init; }
}