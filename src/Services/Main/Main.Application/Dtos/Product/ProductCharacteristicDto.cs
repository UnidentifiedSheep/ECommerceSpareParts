using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Product;

public record ProductCharacteristicDto
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }
}