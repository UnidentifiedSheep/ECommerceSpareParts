using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Product;

public record CreateProductDto
{
    [JsonPropertyName("sku")]
    public required string Sku { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("producerId")]
    public required int ProducerId { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("indicator")]
    public string? Indicator { get; init; }

    [JsonPropertyName("categoryId")]
    public int? CategoryId { get; init; }
}