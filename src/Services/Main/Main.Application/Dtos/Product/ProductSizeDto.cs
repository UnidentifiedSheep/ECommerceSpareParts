using System.Text.Json.Serialization;
using Enums;

namespace Main.Application.Dtos.Product;

public record ProductSizeDto
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }

    [JsonPropertyName("length")]
    public required decimal Length { get; init; }

    [JsonPropertyName("width")]
    public required decimal Width { get; init; }

    [JsonPropertyName("height")]
    public required decimal Height { get; init; }

    [JsonPropertyName("unit")]
    public required DimensionUnit Unit { get; init; }

    [JsonPropertyName("volumeM3")]
    public required decimal VolumeM3 { get; init; }
}